using System;
using System.Drawing;

namespace CrystalTable.Data
{
    /// <summary>
    /// Категория годности кристалла (Bin)
    /// </summary>
    public enum BinCategory
    {
        /// <summary>Не проверен (серый)</summary>
        NotInspected = 0,
        
        /// <summary>Годен (зелёный)</summary>
        Good = 1,
        
        /// <summary>Брак (красный)</summary>
        Defective = 2,
        
        /// <summary>Требует проверки (жёлтый)</summary>
        NeedsReview = 3,
        
        /// <summary>На доработку (оранжевый)</summary>
        Rework = 4,
        
        /// <summary>Краевой/неполный (синий)</summary>
        Edge = 5,
        
        /// <summary>Пользовательская категория 1</summary>
        Custom1 = 10,
        
        /// <summary>Пользовательская категория 2</summary>
        Custom2 = 11,
        
        /// <summary>Пользовательская категория 3</summary>
        Custom3 = 12
    }

    /// <summary>
    /// Настройки цветов и отображения Bin Map
    /// </summary>
    public class BinMapSettings
    {
        /// <summary>
        /// Цвета для каждой категории
        /// </summary>
        public Color GetBinColor(BinCategory bin)
        {
            switch (bin)
            {
                case BinCategory.NotInspected:
                    return Color.FromArgb(Opacity, 180, 180, 180); // Серый
                case BinCategory.Good:
                    return Color.FromArgb(Opacity, 46, 204, 113);  // Зелёный
                case BinCategory.Defective:
                    return Color.FromArgb(Opacity, 231, 76, 60);   // Красный
                case BinCategory.NeedsReview:
                    return Color.FromArgb(Opacity, 241, 196, 15);  // Жёлтый
                case BinCategory.Rework:
                    return Color.FromArgb(Opacity, 230, 126, 34);  // Оранжевый
                case BinCategory.Edge:
                    return Color.FromArgb(Opacity, 52, 152, 219);  // Синий
                case BinCategory.Custom1:
                    return Color.FromArgb(Opacity, 155, 89, 182);  // Фиолетовый
                case BinCategory.Custom2:
                    return Color.FromArgb(Opacity, 26, 188, 156);  // Бирюзовый
                case BinCategory.Custom3:
                    return Color.FromArgb(Opacity, 149, 165, 166); // Серо-голубой
                default:
                    return Color.FromArgb(Opacity, 128, 128, 128);
            }
        }

        /// <summary>
        /// Получить название категории на русском
        /// </summary>
        public static string GetBinName(BinCategory bin)
        {
            switch (bin)
            {
                case BinCategory.NotInspected: return "Не проверен";
                case BinCategory.Good: return "Годен";
                case BinCategory.Defective: return "Брак";
                case BinCategory.NeedsReview: return "Проверить";
                case BinCategory.Rework: return "Доработка";
                case BinCategory.Edge: return "Краевой";
                case BinCategory.Custom1: return "Категория 1";
                case BinCategory.Custom2: return "Категория 2";
                case BinCategory.Custom3: return "Категория 3";
                default: return bin.ToString();
            }
        }

        /// <summary>
        /// Короткое обозначение для отображения на кристалле
        /// </summary>
        public static string GetBinShortName(BinCategory bin)
        {
            switch (bin)
            {
                case BinCategory.NotInspected: return "?";
                case BinCategory.Good: return "✓";
                case BinCategory.Defective: return "✗";
                case BinCategory.NeedsReview: return "!";
                case BinCategory.Rework: return "R";
                case BinCategory.Edge: return "E";
                case BinCategory.Custom1: return "1";
                case BinCategory.Custom2: return "2";
                case BinCategory.Custom3: return "3";
                default: return "?";
            }
        }

        /// <summary>
        /// Прозрачность раскраски (0-255)
        /// </summary>
        public int Opacity { get; set; } = 160;

        /// <summary>
        /// Показывать Bin Map
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Показывать номер/символ категории на кристалле
        /// </summary>
        public bool ShowBinSymbol { get; set; } = false;

        /// <summary>
        /// Показывать легенду
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Позиция легенды
        /// </summary>
        public LegendPosition LegendPosition { get; set; } = LegendPosition.TopRight;
    }

    /// <summary>
    /// Позиция легенды на экране
    /// </summary>
    public enum LegendPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
