using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controllers
{
    /// <summary>
    /// Высокопроизводительный контроллер отрисовки вафли/кристаллов/маршрута.
    /// Комбинирует читаемую структуру (v1) + кэш ресурсов/батчинг (v2) + клип/лимиты/быстрые режимы (v3).
    /// </summary>
    public class DrawingController : IDisposable
    {
        private readonly WaferController _wafer;
        private readonly ZoomPanController _zoom;
        private readonly MouseController _mouse;
        private readonly RoutePreview _routePreview;

        // --- Кэш ресурсов, ключи учитывают DashStyle ---
        private readonly Dictionary<Color, SolidBrush> _brushCache = new Dictionary<Color, SolidBrush>();
        private readonly Dictionary<(Color color, float width, DashStyle style), Pen> _penCache =
            new Dictionary<(Color, float, DashStyle), Pen>();
        private readonly Dictionary<float, Font> _fontCache = new Dictionary<float, Font>();

        // --- Константы отображения ---
        private const float MinZoomForNumbers = 1.5f;
        private const float MinZoomForAllNumbers = 3.0f;
        private const float PointerCrossScale = 0.35f;  // от min(cellW, cellH)
        private const float PointerRingScale = 0.22f;  // от min(cellW, cellH)
        private const float TextSizeThreshold = 0.8f;   // текст должен помещаться в 80% ячейки

        private bool _disposed;

        /// <summary> Текущая позиция указателя в мм (система ваферы). </summary>
        public PointF? PointerMm { get; set; }

        /// <summary> Показывать ли указатель. </summary>
        public bool ShowPointer { get; set; } = true;

        /// <summary> Гибкие настройки производительности/визуализации. </summary>
        public DrawingPerformanceSettings Performance { get; set; } = new DrawingPerformanceSettings();

        public DrawingController(WaferController wafer,
                                 ZoomPanController zoom,
                                 MouseController mouse,
                                 RoutePreview routePreview)
        {
            _wafer = wafer ?? throw new ArgumentNullException(nameof(wafer));
            _zoom = zoom ?? throw new ArgumentNullException(nameof(zoom));
            _mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
            _routePreview = routePreview; // опционально
        }

        /// <summary>
        /// Главный метод отрисовки, вызывается из OnPaint.
        /// </summary>
        public void Draw(Graphics g, int width, int height, bool showRoute)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));
            if (width <= 0 || height <= 0) return;

            ConfigureGraphicsQuality(g);
            g.Clear(Color.White);

            // Сохраняем трансформации/клип для мира
            var state = g.Save();
            try
            {
                ApplyTransformations(g);
                DrawWorld(g, width, height, showRoute);
            }
            finally
            {
                g.Restore(state);
            }

            // UI поверх (экранные координаты)
            DrawOverlay(g, width, height);
        }

        // ---------- Настройки качества ----------
        private void ConfigureGraphicsQuality(Graphics g)
        {
            if (Performance.HighQualityRendering)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                g.CompositingQuality = CompositingQuality.HighSpeed;
            }
        }

        // ---------- Трансформации ----------
        private void ApplyTransformations(Graphics g)
        {
            g.TranslateTransform(_zoom.PanOffset.X, _zoom.PanOffset.Y);
            g.ScaleTransform(_zoom.ZoomFactor, _zoom.ZoomFactor);
        }

        // ---------- Мир (вафля, кристаллы, маршрут, указатель) ----------
        private void DrawWorld(Graphics g, int width, int height, bool showRoute)
        {
            _wafer.AutoSetScaleFactor(width, height);

            DrawWafer(g, width, height);

            _wafer.BuildCrystalsCached();
            DrawCrystalsOptimized(g, width, height);

            if (showRoute && _routePreview != null)
            {
                var center = new PointF(width / 2f, height / 2f);
                _routePreview.DrawRoutePreview(
                    g,
                    CrystalManager.Instance.Crystals,
                    _wafer.ScaleFactor,
                    center.X, center.Y);
            }

            DrawPointer(g, width, height);
        }

        private void DrawWafer(Graphics g, int width, int height)
        {
            var center = new PointF(width / 2f, height / 2f);
            float rMm = _wafer.WaferDiameter / 2f;
            float rPx = rMm * _wafer.ScaleFactor;

            var bounds = new RectangleF(center.X - rPx, center.Y - rPx, rPx * 2, rPx * 2);

            if (_wafer.WaferDisplayMode)
            {
                using (var pen = GetPen(Color.Black, 2f))
                    g.DrawEllipse(pen, bounds);
            }
            else
            {
                using (var fill = GetBrush(Color.LightGreen))
                using (var pen = GetPen(Color.DarkGreen, 2f))
                {
                    g.FillEllipse(fill, bounds);
                    g.DrawEllipse(pen, bounds);
                }
            }
        }

        private void DrawCrystalsOptimized(Graphics g, int width, int height)
        {
            var cell = GetDisplayCellSize();
            var center = new PointF(width / 2f, height / 2f);
            var visible = GetVisibleArea(width, height);

            // Необязательный клип по окружности вафли: ускоряет, если сетка большая
            Region prevClip = null;
            if (Performance.ClipToWafer)
            {
                prevClip = g.Clip?.Clone();
                using (var gp = new GraphicsPath())
                {
                    float rPx = (_wafer.WaferDiameter / 2f) * _wafer.ScaleFactor;
                    gp.AddEllipse(new RectangleF(center.X - rPx, center.Y - rPx, rPx * 2, rPx * 2));
                    g.SetClip(gp, CombineMode.Intersect);
                }
            }

            // Грубое ускорение линий прямоугольников
            var oldSmooth = g.SmoothingMode;
            if (Performance.FastGridLines) g.SmoothingMode = SmoothingMode.None;

            int emitted = 0;
            // Группируем по состоянию рендеринга (выбран / показывать номер)
            var groups = new Dictionary<CrystalRenderState, List<Crystal>>();

            foreach (var c in CrystalManager.Instance.Crystals)
            {
                UpdateCrystalDisplayCoordinates(c, center, cell);
                if (!IsInVisibleArea(c, visible)) continue;

                var state = new CrystalRenderState(
                    _mouse.SelectedCrystals.Contains(c.Index),
                    ShouldShowNumber(c, cell));

                if (!groups.TryGetValue(state, out var list))
                    groups[state] = list = new List<Crystal>();
                list.Add(c);

                emitted++;
                if (emitted >= Performance.MaxVisibleCrystals) break;
            }

            // Батч-рендеринг
            foreach (var kv in groups)
                DrawCrystalGroup(g, kv.Key, kv.Value, cell);

            g.SmoothingMode = oldSmooth;

            if (Performance.ClipToWafer)
            {
                g.Clip = prevClip;
                prevClip?.Dispose();
            }
        }

        private SizeF GetDisplayCellSize()
        {
            float w = (_wafer.CrystalWidthRaw / 1000f) * _wafer.ScaleFactor;
            float h = (_wafer.CrystalHeightRaw / 1000f) * _wafer.ScaleFactor;
            return new SizeF(w, h);
        }

        private void UpdateCrystalDisplayCoordinates(Crystal c, PointF center, SizeF cell)
        {
            float sx = c.RealX * _wafer.ScaleFactor + center.X;
            float sy = c.RealY * _wafer.ScaleFactor + center.Y;
            float hw = cell.Width / 2f;
            float hh = cell.Height / 2f;

            c.DisplayX = sx; c.DisplayY = sy;
            c.DisplayLeft = sx - hw; c.DisplayRight = sx + hw;
            c.DisplayTop = sy - hh; c.DisplayBottom = sy + hh;
        }

        private bool ShouldShowNumber(Crystal c, SizeF cell)
        {
            float zoom = _zoom.ZoomFactor;
            if (zoom < MinZoomForNumbers) return false;

            // Дешёвая ранняя отсечка: если ячейка слишком маленькая — не меряем текст вовсе
            if (cell.Width < Performance.MinCellPixelsForNumbers ||
                cell.Height < Performance.MinCellPixelsForNumbers)
                return false;

            if (zoom >= MinZoomForAllNumbers) return true;
            return (c.Index % 4) == 0;
        }

        private void DrawCrystalGroup(Graphics g, CrystalRenderState state, List<Crystal> list, SizeF cell)
        {
            var (fillColor, borderColor, borderWidth) = state.IsSelected
                ? (Color.Yellow, Color.DarkBlue, 2f)
                : (Color.Empty, Color.Blue, 1f);

            using var fill = (fillColor == Color.Empty) ? null : GetBrush(fillColor);
            using var pen = GetPen(borderColor, borderWidth);

            foreach (var c in list)
            {
                // Заливка
                if (fill != null) g.FillRectangle(fill, c.DisplayLeft, c.DisplayTop, cell.Width, cell.Height);

                // Контур (float-координаты без округления)
                g.DrawRectangle(pen, c.DisplayLeft, c.DisplayTop, cell.Width, cell.Height);

                // Номер
                if (state.ShowNumber) DrawCrystalNumber(g, c, cell);
            }
        }

        private void DrawCrystalNumber(Graphics g, Crystal c, SizeF cell)
        {
            float zoom = _zoom.ZoomFactor;
            // Адаптивный квантованный размер шрифта
            float fs = QuantizeFontSize(Math.Max(6f, Math.Min(12f, 8f * zoom)));

            using var font = GetFont(fs);
            using var brush = GetBrush(Color.Black);

            string text = c.Index.ToString();
            var size = g.MeasureString(text, font);
            if (size.Width > cell.Width * TextSizeThreshold ||
                size.Height > cell.Height * TextSizeThreshold)
                return;

            float tx = c.DisplayX - size.Width / 2f;
            float ty = c.DisplayY - size.Height / 2f;
            g.DrawString(text, font, brush, new PointF(tx, ty));
        }

        private float QuantizeFontSize(float s)
        {
            var q = (float)Math.Round(s * 2f) / 2f; // шаг 0.5 pt
            return Math.Max(6f, Math.Min(18f, q));
        }

        private void DrawPointer(Graphics g, int width, int height)
        {
            if (!ShowPointer || !PointerMm.HasValue) return;

            var center = new PointF(width / 2f, height / 2f);
            float scale = _wafer.ScaleFactor;
            var mm = PointerMm.Value;

            var screen = new PointF(center.X + mm.X * scale, center.Y + mm.Y * scale);

            // Геометрия от min(cellW, cellH)
            float cellWmm = Math.Max(_wafer.CrystalWidthRaw / 1000f, 0.05f);
            float cellHmm = Math.Max(_wafer.CrystalHeightRaw / 1000f, 0.05f);
            float cellMin = Math.Min(cellWmm, cellHmm);

            float crossHalf = PointerCrossScale * cellMin * scale;
            float ringR = PointerRingScale * cellMin * scale;

            using var solid = GetPen(Color.Red, 2f);
            using var dashed = GetPen(Color.FromArgb(160, Color.Red), 1f, DashStyle.Dot);
            using var fill = GetBrush(Color.FromArgb(30, Color.Red));

            var ring = new RectangleF(screen.X - ringR, screen.Y - ringR, ringR * 2, ringR * 2);
            g.FillEllipse(fill, ring);
            g.DrawEllipse(solid, ring);

            g.DrawLine(solid, screen.X - crossHalf, screen.Y, screen.X + crossHalf, screen.Y);
            g.DrawLine(solid, screen.X, screen.Y - crossHalf, screen.X, screen.Y + crossHalf);

            if (Performance.ShowPointerGuides)
            {
                float rPx = (_wafer.WaferDiameter / 2f) * scale;
                float left = center.X - rPx, right = center.X + rPx;
                float top = center.Y - rPx, bottom = center.Y + rPx;
                g.DrawLine(dashed, left, screen.Y, right, screen.Y);
                g.DrawLine(dashed, screen.X, top, screen.X, bottom);
            }
        }

        // ---------- Overlay (экранные координаты) ----------
        private void DrawOverlay(Graphics g, int width, int height)
        {
            DrawSelection(g);
            DrawZoomInfo(g, width, height);
        }

        private void DrawSelection(Graphics g)
        {
            var r = _mouse.GetSelectionRectangle();
            if (r.IsEmpty) return;

            using var pen = GetPen(Color.FromArgb(128, Color.Blue), 2f, DashStyle.Dash);
            using var br = GetBrush(Color.FromArgb(30, Color.Blue));
            g.FillRectangle(br, r);
            g.DrawRectangle(pen, r);
        }

        private void DrawZoomInfo(Graphics g, int width, int height)
        {
            if (!Performance.ShowZoomInfo) return;

            string text = $"Масштаб: {_zoom.ZoomFactor:F1}x";
            using var font = GetFont(QuantizeFontSize(10f));
            using var fg = GetBrush(Color.Black);
            using var bg = GetBrush(Color.FromArgb(200, Color.White));

            var size = g.MeasureString(text, font);
            float x = width - size.Width - 10;
            float y = height - size.Height - 10;
            g.FillRectangle(bg, x - 5, y - 5, size.Width + 10, size.Height + 10);
            g.DrawString(text, font, fg, new PointF(x, y));
        }

        // ---------- Геометрия видимости ----------
        private RectangleF GetVisibleArea(int width, int height)
        {
            var tl = _zoom.TransformPoint(new PointF(0, 0));
            var br = _zoom.TransformPoint(new PointF(width, height));
            return new RectangleF(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);
        }

        private static bool IsInVisibleArea(Crystal c, RectangleF area)
        {
            return c.DisplayLeft <= area.Right &&
                   c.DisplayRight >= area.Left &&
                   c.DisplayTop <= area.Bottom &&
                   c.DisplayBottom >= area.Top;
        }

        // ---------- Кэш ресурсов ----------
        private SolidBrush GetBrush(Color color)
        {
            if (!_brushCache.TryGetValue(color, out var b))
                _brushCache[color] = b = new SolidBrush(color);
            return b;
        }

        private Pen GetPen(Color color, float width, DashStyle style = DashStyle.Solid)
        {
            var key = (color, width, style);
            if (!_penCache.TryGetValue(key, out var p))
            {
                p = new Pen(color, width) { DashStyle = style };
                _penCache[key] = p;
            }
            return p;
        }

        private Font GetFont(float size)
        {
            if (!_fontCache.TryGetValue(size, out var f))
                _fontCache[size] = f = new Font("Arial", size);
            return f;
        }

        // ---------- IDisposable ----------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                foreach (var b in _brushCache.Values) b?.Dispose();
                foreach (var p in _penCache.Values) p?.Dispose();
                foreach (var f in _fontCache.Values) f?.Dispose();
                _brushCache.Clear();
                _penCache.Clear();
                _fontCache.Clear();
            }
            _disposed = true;
        }
    }

    /// <summary> Настройки производительности/визуализации. </summary>
    public class DrawingPerformanceSettings
    {
        public bool HighQualityRendering { get; set; } = true;
        public bool ShowPointerGuides { get; set; } = true;
        public bool ShowZoomInfo { get; set; } = true;

        /// <summary> Клип по окружности вафли. </summary>
        public bool ClipToWafer { get; set; } = true;

        /// <summary> Сглаживание линий прямоугольников отключать для скорости. </summary>
        public bool FastGridLines { get; set; } = true;

        /// <summary> Лимит кристаллов за кадр. </summary>
        public int MaxVisibleCrystals { get; set; } = 10000;

        /// <summary> Минимальные размеры ячейки (px) для рисования номеров. </summary>
        public float MinCellPixelsForNumbers { get; set; } = 12f;
    }

    /// <summary> Состояние рендеринга кристалла (для батч-рендеринга). </summary>
    public readonly struct CrystalRenderState : IEquatable<CrystalRenderState>
    {
        public CrystalRenderState(bool isSelected, bool showNumber)
        {
            IsSelected = isSelected;
            ShowNumber = showNumber;
        }

        public bool IsSelected { get; }
        public bool ShowNumber { get; }

        public bool Equals(CrystalRenderState other) =>
            IsSelected == other.IsSelected && ShowNumber == other.ShowNumber;

        public override bool Equals(object obj) =>
            obj is CrystalRenderState s && Equals(s);

        public override int GetHashCode()
        {
            unchecked { return (IsSelected.GetHashCode() * 397) ^ ShowNumber.GetHashCode(); }
        }
    }
}
