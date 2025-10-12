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
    }
}
