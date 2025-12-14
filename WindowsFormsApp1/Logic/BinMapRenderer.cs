using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CrystalTable.Data;

namespace CrystalTable.Logic
{
    /// <summary>
    /// Рендерер Bin Map — отрисовка карты годности кристаллов
    /// </summary>
    public class BinMapRenderer : IDisposable
    {
        private readonly BinMapSettings settings;
        private readonly Dictionary<BinCategory, SolidBrush> brushCache;
        private Font symbolFont;
        private Font legendFont;

        public BinMapRenderer() : this(new BinMapSettings())
        {
        }

        public BinMapRenderer(BinMapSettings settings)
        {
            this.settings = settings ?? new BinMapSettings();
            brushCache = new Dictionary<BinCategory, SolidBrush>();
            UpdateBrushCache();
        }

        /// <summary>
        /// Настройки Bin Map
        /// </summary>
        public BinMapSettings Settings => settings;

        /// <summary>
        /// Обновить кэш кистей при изменении настроек
        /// </summary>
        public void UpdateBrushCache()
        {
            // Очистить старый кэш
            foreach (var brush in brushCache.Values)
            {
                brush?.Dispose();
            }
            brushCache.Clear();

            // Создать новые кисти для всех категорий
            foreach (BinCategory bin in Enum.GetValues(typeof(BinCategory)))
            {
                var color = settings.GetBinColor(bin);
                brushCache[bin] = new SolidBrush(color);
            }

            // Обновить шрифты
            symbolFont?.Dispose();
            legendFont?.Dispose();
            symbolFont = new Font("Segoe UI", 7f, FontStyle.Bold);
            legendFont = new Font("Segoe UI", 9f);
        }

        /// <summary>
        /// Отрисовать Bin Map поверх кристаллов
        /// </summary>
        public void Draw(Graphics g, IReadOnlyList<Crystal> crystals, RectangleF viewBounds)
        {
            if (!settings.Enabled || crystals == null || crystals.Count == 0)
                return;

            // Отрисовка раскраски кристаллов (в трансформированных координатах)
            foreach (var crystal in crystals)
            {
                // Пропускаем кристаллы вне области видимости
                if (crystal.DisplayRight < viewBounds.Left ||
                    crystal.DisplayLeft > viewBounds.Right ||
                    crystal.DisplayBottom < viewBounds.Top ||
                    crystal.DisplayTop > viewBounds.Bottom)
                {
                    continue;
                }

                DrawCrystalBin(g, crystal);
            }
        }

        /// <summary>
        /// Отрисовать раскраску одного кристалла
        /// </summary>
        private void DrawCrystalBin(Graphics g, Crystal crystal)
        {
            // Для непроверенных не рисуем раскраску
            if (crystal.Bin == BinCategory.NotInspected)
                return;

            float left = crystal.DisplayLeft;
            float top = crystal.DisplayTop;
            float width = crystal.DisplayRight - crystal.DisplayLeft;
            float height = crystal.DisplayBottom - crystal.DisplayTop;

            if (width <= 0 || height <= 0)
                return;

            // Получить кисть из кэша
            if (brushCache.TryGetValue(crystal.Bin, out var brush))
            {
                g.FillRectangle(brush, left, top, width, height);
            }

            // Отрисовка символа категории при достаточном размере
            if (settings.ShowBinSymbol && width > 15 && height > 15)
            {
                string symbol = BinMapSettings.GetBinShortName(crystal.Bin);
                var size = g.MeasureString(symbol, symbolFont);
                
                float x = left + (width - size.Width) / 2;
                float y = top + (height - size.Height) / 2;

                using (var textBrush = new SolidBrush(Color.White))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                {
                    g.DrawString(symbol, symbolFont, shadowBrush, x + 1, y + 1);
                    g.DrawString(symbol, symbolFont, textBrush, x, y);
                }
            }
        }

        /// <summary>
        /// Отрисовать легенду (вызывать ПОСЛЕ g.Restore для фиксированной позиции на экране)
        /// </summary>
        public void DrawLegend(Graphics g, IReadOnlyList<Crystal> crystals, RectangleF screenBounds)
        {
            if (!settings.Enabled || !settings.ShowLegend || crystals == null || crystals.Count == 0)
                return;

            // Подсчёт кристаллов по категориям
            var counts = GetBinCounts(crystals);
            int totalGood = counts.ContainsKey(BinCategory.Good) ? counts[BinCategory.Good] : 0;

            // Размеры легенды
            float padding = 8;
            float lineHeight = 18;
            float colorBoxSize = 12;
            
            // Определяем видимые категории (те, у которых есть кристаллы)
            var visibleBins = counts.Where(kv => kv.Value > 0).OrderBy(kv => (int)kv.Key).ToList();
            
            float legendWidth = 160;
            float legendHeight = padding * 2 + (visibleBins.Count + 2) * lineHeight;

            // Позиция легенды на ЭКРАНЕ (фиксированная)
            float legendX, legendY;
            switch (settings.LegendPosition)
            {
                case LegendPosition.TopLeft:
                    legendX = screenBounds.Left + 50; // отступ от zoom indicator
                    legendY = screenBounds.Top + 10;
                    break;
                case LegendPosition.TopRight:
                    legendX = screenBounds.Right - legendWidth - 10;
                    legendY = screenBounds.Top + 10;
                    break;
                case LegendPosition.BottomLeft:
                    legendX = screenBounds.Left + 10;
                    legendY = screenBounds.Bottom - legendHeight - 10;
                    break;
                case LegendPosition.BottomRight:
                default:
                    legendX = screenBounds.Right - legendWidth - 10;
                    legendY = screenBounds.Bottom - legendHeight - 10;
                    break;
            }

            // Фон легенды
            using (var bgBrush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
            using (var borderPen = new Pen(Color.FromArgb(180, 180, 180), 1))
            {
                var rect = new RectangleF(legendX, legendY, legendWidth, legendHeight);
                
                using (var path = CreateRoundedRectangle(rect, 6))
                {
                    g.FillPath(bgBrush, path);
                    g.DrawPath(borderPen, path);
                }
            }

            float currentY = legendY + padding;

            // Заголовок
            using (var titleBrush = new SolidBrush(Color.FromArgb(64, 64, 64)))
            using (var titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                g.DrawString("Bin Map", titleFont, titleBrush, legendX + padding, currentY);
                currentY += lineHeight;
            }

            // Категории
            using (var textBrush = new SolidBrush(Color.FromArgb(64, 64, 64)))
            {
                foreach (var kv in visibleBins)
                {
                    var bin = kv.Key;
                    var count = kv.Value;

                    // Цветной квадрат
                    var color = settings.GetBinColor(bin);
                    using (var colorBrush = new SolidBrush(Color.FromArgb(255, color)))
                    using (var borderPen = new Pen(Color.FromArgb(100, 100, 100), 1))
                    {
                        g.FillRectangle(colorBrush, legendX + padding, currentY + 2, colorBoxSize, colorBoxSize);
                        g.DrawRectangle(borderPen, legendX + padding, currentY + 2, colorBoxSize, colorBoxSize);
                    }

                    // Название и количество
                    string text = $"{BinMapSettings.GetBinName(bin)}: {count}";
                    g.DrawString(text, legendFont, textBrush, legendX + padding + colorBoxSize + 6, currentY);
                    
                    currentY += lineHeight;
                }

                // Yield (выход годных)
                currentY += 4;
                
                int totalForYield = crystals.Count - (counts.ContainsKey(BinCategory.Edge) ? counts[BinCategory.Edge] : 0);
                float yield = totalForYield > 0 ? (float)totalGood / totalForYield * 100f : 0f;
                
                using (var yieldFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                {
                    Color yieldColor = yield >= 90 ? Color.FromArgb(39, 174, 96) :
                                       yield >= 70 ? Color.FromArgb(241, 196, 15) :
                                                     Color.FromArgb(231, 76, 60);
                    using (var yieldBrush = new SolidBrush(yieldColor))
                    {
                        g.DrawString($"Yield: {yield:F1}%", yieldFont, yieldBrush, legendX + padding, currentY);
                    }
                }
            }
        }

        /// <summary>
        /// Подсчёт кристаллов по категориям
        /// </summary>
        public static Dictionary<BinCategory, int> GetBinCounts(IReadOnlyList<Crystal> crystals)
        {
            var counts = new Dictionary<BinCategory, int>();
            
            // Инициализация всех категорий нулями
            foreach (BinCategory bin in Enum.GetValues(typeof(BinCategory)))
            {
                counts[bin] = 0;
            }

            if (crystals == null)
                return counts;

            // Подсчёт
            foreach (var crystal in crystals)
            {
                counts[crystal.Bin]++;
            }

            return counts;
        }

        /// <summary>
        /// Вычислить Yield (выход годных) в процентах
        /// </summary>
        public static float CalculateYield(IReadOnlyList<Crystal> crystals)
        {
            if (crystals == null || crystals.Count == 0)
                return 0f;

            var counts = GetBinCounts(crystals);
            
            int good = counts[BinCategory.Good];
            int total = crystals.Count - counts[BinCategory.Edge]; // Краевые не учитываем
            
            return total > 0 ? (float)good / total * 100f : 0f;
        }

        /// <summary>
        /// Создать путь со скруглёнными углами
        /// </summary>
        private static GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        public void Dispose()
        {
            foreach (var brush in brushCache.Values)
            {
                brush?.Dispose();
            }
            brushCache.Clear();
            
            symbolFont?.Dispose();
            legendFont?.Dispose();
        }
    }
}
