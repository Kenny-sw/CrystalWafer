using System;
using System.Xml.Serialization;

namespace CrystalTable.Data
{
    /// <summary>
    /// Шаблон карты пластины для повторного использования
    /// </summary>
    [Serializable]
    public class WaferTemplate
    {
        [XmlElement]
        public string TemplateName { get; set; }

        [XmlElement]
        public float DiameterMm { get; set; }

        [XmlElement]
        public float CrystalWidthMm { get; set; }

        [XmlElement]
        public float CrystalHeightMm { get; set; }

        [XmlElement]
        public float StreetMm { get; set; }

        [XmlElement]
        public float OffsetXMm { get; set; }

        [XmlElement]
        public float OffsetYMm { get; set; }

        [XmlElement]
        public bool MirrorX { get; set; }

        [XmlElement]
        public bool MirrorY { get; set; }

        [XmlElement]
        public float EdgeExclusionMm { get; set; }

        [XmlElement]
        public DateTime CreatedDate { get; set; }

        [XmlElement]
        public DateTime ModifiedDate { get; set; }

        public WaferTemplate()
        {
            TemplateName = "Без названия";
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public WaferTemplate Clone()
        {
            return new WaferTemplate
            {
                TemplateName = this.TemplateName,
                DiameterMm = this.DiameterMm,
                CrystalWidthMm = this.CrystalWidthMm,
                CrystalHeightMm = this.CrystalHeightMm,
                StreetMm = this.StreetMm,
                OffsetXMm = this.OffsetXMm,
                OffsetYMm = this.OffsetYMm,
                MirrorX = this.MirrorX,
                MirrorY = this.MirrorY,
                EdgeExclusionMm = this.EdgeExclusionMm,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }
    }
}
