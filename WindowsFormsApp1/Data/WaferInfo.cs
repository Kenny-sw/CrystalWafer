using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CrystalTable.Data
{
    [Serializable]
    public class WaferInfo
    {
        [XmlElement]
        public uint SizeX { get; set; }

        [XmlElement]
        public uint SizeY { get; set; }

        [XmlElement]
        public uint WaferDiameter { get; set; }
    }
}
