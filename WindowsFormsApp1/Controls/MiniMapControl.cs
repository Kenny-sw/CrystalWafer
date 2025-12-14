using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CrystalTable.Data;
using CrystalTable.Logic;

namespace CrystalTable.Controls
{
    /// <summary>
    /// Миникарта (Overview) — показывает всю пластину с индикатором области просмотра
    /// </summary>
    public class MiniMapControl : UserControl
    {
        private Bitmap cachedBitmap;
        private int cachedCrystalCount;
        private float cachedDiameter;
        
        private RectangleF viewportRect;
        private PointF pointerPosition;
        private bool isDragging;
        
        /// <summary>
        /// Событие при клике на миникарту для навигации
        /// </summary>
        public event EventHandler<MiniMapClickEventArgs> NavigationRequested;

        /// <summary>
        /// Диаметр пластины в мм
        /// </summary>
        public float WaferDiameter { get; set; } = 150f;

        /// <summary>
        /// Показывать Bin Map раскраску
        /// </summary>
        public bool ShowBinColors { get; set; } = true;

        /// <summary>
        /// Показывать позицию указателя
        /// </summary>
        public bool ShowPointer { get; set; } = true;

        /// <summary>
        /// Показывать область просмотра
        /// </summary>
        public bool ShowViewport { get; set; } = true;

        public MiniMapControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(240, 240, 240);
            BorderStyle = BorderStyle.FixedSingle;
            MinimumSize = new Size(100, 100);
        }

        /// <summary>
        /// Установить область просмотра (viewport) в координатах мм
        /// </summary>
        public void SetViewport(float centerX, float centerY, float widthMm, float heightMm)
        {
            viewportRect = new RectangleF(
                centerX - widthMm / 2,
                centerY - heightMm / 2,
                widthMm,
                heightMm);
            Invalidate();
        }

        /// <summary>
        /// Установить позицию указателя в координатах мм
        /// </summary>
        public void SetPointerPosition(float xMm, float yMm)
        {
            pointerPosition = new PointF(xMm, yMm);
            Invalidate();
        }

        /// <summary>
        /// Принудительно обновить кэш
        /// </summary>
        public void InvalidateCache()
        {
            cachedBitmap?.Dispose();
            cachedBitmap = null;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            if (WaferDiameter <= 0)
                return;

            // Вычисляем масштаб для вписывания пластины
            float padding = 10;
            float availableSize = Math.Min(Width, Height) - padding * 2;
            float scale = availableSize / WaferDiameter;
            
            float centerX = Width / 2f;
            float centerY = Height / 2f;

            // Рисуем кэшированную карту или создаём новую
            DrawWaferMap(g, centerX, centerY, scale);

            // Рисуем область просмотра
            if (ShowViewport)
            {
                DrawViewport(g, centerX, centerY, scale);
            }

            // Рисуем указатель
            if (ShowPointer)
            {
                DrawPointer(g, centerX, centerY, scale);
            }

            // Рамка
            using (var pen = new Pen(Color.FromArgb(180, 180, 180), 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void DrawWaferMap(Graphics g, float centerX, float centerY, float scale)
        {
            var crystals = CrystalManager.Instance.Crystals;
            bool needsRebuild = cachedBitmap == null ||
                                cachedCrystalCount != crystals.Count ||
                                Math.Abs(cachedDiameter - WaferDiameter) > 0.1f;

            if (needsRebuild)
            {
                RebuildCache(crystals, scale);
            }

            if (cachedBitmap != null)
            {
                float bmpX = centerX - cachedBitmap.Width / 2f;
                float bmpY = centerY - cachedBitmap.Height / 2f;
                g.DrawImage(cachedBitmap, bmpX, bmpY);
            }
            else
            {
                // Если нет кристаллов — просто рисуем пустую пластину
                float radius = (WaferDiameter / 2f) * scale;
                using (var fill = new SolidBrush(Color.FromArgb(200, 220, 200)))
                using (var pen = new Pen(Color.FromArgb(100, 140, 100), 2))
                {
                    g.FillEllipse(fill, centerX - radius, centerY - radius, radius * 2, radius * 2);
                    g.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);
                }
            }
        }

        private void RebuildCache(List<Crystal> crystals, float scale)
        {
            cachedBitmap?.Dispose();
            
            int size = (int)(WaferDiameter * scale) + 4;
            if (size <= 0) size = 100;
            
            cachedBitmap = new Bitmap(size, size);
            cachedCrystalCount = crystals.Count;
            cachedDiameter = WaferDiameter;

            using (var g = Graphics.FromImage(cachedBitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float cx = size / 2f;
                float cy = size / 2f;
                float radius = (WaferDiameter / 2f) * scale;

                // Фон пластины
                using (var fill = new SolidBrush(Color.FromArgb(180, 200, 180)))
                using (var pen = new Pen(Color.FromArgb(100, 140, 100), 1.5f))
                {
                    g.FillEllipse(fill, cx - radius, cy - radius, radius * 2, radius * 2);
                    g.DrawEllipse(pen, cx - radius, cy - radius, radius * 2, radius * 2);
                }

                // Клиппинг по кругу
                using (var clipPath = new GraphicsPath())
                {
                    clipPath.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2);
                    g.SetClip(clipPath);

                    // Рисуем кристаллы
                    foreach (var crystal in crystals)
                    {
                        float x = crystal.RealX * scale + cx;
                        float y = crystal.RealY * scale + cy;
                        float w = crystal.WidthMm * scale;
                        float h = crystal.HeightMm * scale;

                        // Минимальный размер для видимости
                        if (w < 1) w = 1;
                        if (h < 1) h = 1;

                        Color fillColor;
                        if (ShowBinColors && crystal.Bin != BinCategory.NotInspected)
                        {
                            fillColor = GetBinColor(crystal.Bin);
                        }
                        else
                        {
                            fillColor = crystal.PlacementStatus == CrystalPlacementStatus.Full
                                ? Color.FromArgb(80, 140, 80)
                                : Color.FromArgb(120, 160, 120);
                        }

                        using (var brush = new SolidBrush(fillColor))
                        {
                            g.FillRectangle(brush, x - w / 2, y - h / 2, w, h);
                        }
                    }

                    g.ResetClip();
                }
            }
        }

        private Color GetBinColor(BinCategory bin)
        {
            switch (bin)
            {
                case BinCategory.Good: return Color.FromArgb(46, 204, 113);
                case BinCategory.Defective: return Color.FromArgb(231, 76, 60);
                case BinCategory.NeedsReview: return Color.FromArgb(241, 196, 15);
                case BinCategory.Rework: return Color.FromArgb(230, 126, 34);
                case BinCategory.Edge: return Color.FromArgb(52, 152, 219);
                default: return Color.FromArgb(150, 150, 150);
            }
        }

        private void DrawViewport(Graphics g, float centerX, float centerY, float scale)
        {
            if (viewportRect.Width <= 0 || viewportRect.Height <= 0)
                return;

            float x = viewportRect.X * scale + centerX;
            float y = viewportRect.Y * scale + centerY;
            float w = viewportRect.Width * scale;
            float h = viewportRect.Height * scale;

            using (var pen = new Pen(Color.FromArgb(200, 52, 152, 219), 2))
            using (var fill = new SolidBrush(Color.FromArgb(30, 52, 152, 219)))
            {
                pen.DashStyle = DashStyle.Dash;
                g.FillRectangle(fill, x, y, w, h);
                g.DrawRectangle(pen, x, y, w, h);
            }
        }

        private void DrawPointer(Graphics g, float centerX, float centerY, float scale)
        {
            float x = pointerPosition.X * scale + centerX;
            float y = pointerPosition.Y * scale + centerY;
            float size = 6;

            using (var pen = new Pen(Color.Red, 2))
            using (var fill = new SolidBrush(Color.FromArgb(150, Color.Red)))
            {
                g.FillEllipse(fill, x - size / 2, y - size / 2, size, size);
                g.DrawLine(pen, x - size, y, x + size, y);
                g.DrawLine(pen, x, y - size, x, y + size);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                HandleClick(e.Location);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            if (isDragging && e.Button == MouseButtons.Left)
            {
                HandleClick(e.Location);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isDragging = false;
        }

        private void HandleClick(Point location)
        {
            if (WaferDiameter <= 0)
                return;

            float padding = 10;
            float availableSize = Math.Min(Width, Height) - padding * 2;
            float scale = availableSize / WaferDiameter;

            float centerX = Width / 2f;
            float centerY = Height / 2f;

            // Преобразуем экранные координаты в мм
            float xMm = (location.X - centerX) / scale;
            float yMm = (location.Y - centerY) / scale;

            // Проверяем что клик внутри пластины
            float radius = WaferDiameter / 2f;
            if (xMm * xMm + yMm * yMm <= radius * radius)
            {
                NavigationRequested?.Invoke(this, new MiniMapClickEventArgs(xMm, yMm));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cachedBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Аргументы события клика на миникарту
    /// </summary>
    public class MiniMapClickEventArgs : EventArgs
    {
        public float X { get; }
        public float Y { get; }

        public MiniMapClickEventArgs(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
