using System;
using System.Drawing;
using System.Xml.Serialization;

namespace CrystalTable.Data
{
    public class Crystal
    {
        public int Index { get; set; }

        [XmlIgnore]
        public Color Color { get; set; }

        [XmlElement("Color")]
        public int ColorArgb
        {
            get => Color.ToArgb();
            set => Color = Color.FromArgb(value);
        }

        public float RealX { get; set; }
        public float RealY { get; set; }
        public float Z { get; set; }

        public float WidthMm { get; set; }
        public float HeightMm { get; set; }

        [XmlIgnore]
        public CrystalPlacementStatus PlacementStatus { get; set; } = CrystalPlacementStatus.Full;

        [XmlElement("Status")]
        public string StatusString
        {
            get => PlacementStatus.ToString();
            set
            {
                if (Enum.TryParse(value, true, out CrystalPlacementStatus parsed))
                {
                    PlacementStatus = parsed;
                }
                else
                {
                    PlacementStatus = CrystalPlacementStatus.Full;
                }
            }
        }

        // ========== Bin Map (карта годности) ==========
        
        /// <summary>
        /// Категория годности кристалла
        /// </summary>
        [XmlIgnore]
        public BinCategory Bin { get; set; } = BinCategory.NotInspected;

        /// <summary>
        /// Сериализация категории годности
        /// </summary>
        [XmlElement("Bin")]
        public string BinString
        {
            get => Bin.ToString();
            set
            {
                if (Enum.TryParse(value, true, out BinCategory parsed))
                {
                    Bin = parsed;
                }
                else
                {
                    Bin = BinCategory.NotInspected;
                }
            }
        }

        /// <summary>
        /// Время проверки кристалла
        /// </summary>
        [XmlIgnore]
        public DateTime? InspectionTime { get; set; }

        /// <summary>
        /// Сериализация времени проверки
        /// </summary>
        [XmlElement("InspectionTime")]
        public string InspectionTimeString
        {
            get => InspectionTime?.ToString("o") ?? string.Empty;
            set
            {
                if (DateTime.TryParse(value, out DateTime parsed))
                {
                    InspectionTime = parsed;
                }
                else
                {
                    InspectionTime = null;
                }
            }
        }

        /// <summary>
        /// Заметки оператора по кристаллу
        /// </summary>
        public string InspectionNotes { get; set; }

        // ========== Координаты отображения ==========

        [XmlIgnore]
        public float DisplayX { get; set; }
        [XmlIgnore]
        public float DisplayY { get; set; }

        [XmlIgnore]
        public float DisplayLeft { get; set; }
        [XmlIgnore]
        public float DisplayRight { get; set; }
        [XmlIgnore]
        public float DisplayTop { get; set; }
        [XmlIgnore]
        public float DisplayBottom { get; set; }

        // ========== Вспомогательные методы ==========

        /// <summary>
        /// Установить категорию годности с фиксацией времени
        /// </summary>
        public void SetBin(BinCategory category)
        {
            Bin = category;
            InspectionTime = DateTime.Now;
        }

        /// <summary>
        /// Сбросить результаты проверки
        /// </summary>
        public void ResetInspection()
        {
            Bin = BinCategory.NotInspected;
            InspectionTime = null;
            InspectionNotes = null;
        }

        /// <summary>
        /// Проверен ли кристалл
        /// </summary>
        [XmlIgnore]
        public bool IsInspected => Bin != BinCategory.NotInspected;
    }
}
