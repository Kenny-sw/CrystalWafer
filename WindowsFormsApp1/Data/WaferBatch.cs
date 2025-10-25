using System;
using System.Xml.Serialization;

namespace CrystalTable.Data
{
    /// <summary>
    /// Информация о партии пластин
    /// </summary>
    [Serializable]
    public class WaferBatch
    {
        [XmlElement]
        public string BatchName { get; set; }

        [XmlElement]
        public string Notes { get; set; }

        [XmlElement]
        public string TemplateName { get; set; }

        [XmlElement]
        public int WaferCount { get; set; }

        [XmlElement]
        public DateTime CreatedDate { get; set; }

        [XmlElement]
        public DateTime ModifiedDate { get; set; }

        [XmlElement]
        public bool IsArchived { get; set; }

        public WaferBatch()
        {
            BatchName = "Партия";
            Notes = string.Empty;
            WaferCount = 0;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            IsArchived = false;
        }
    }
}
