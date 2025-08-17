using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CrystalTable.Data;

namespace CrystalTable.Logic
{
    /// <summary>
    ///     Унифицированный сервис ввода‑вывода.
    ///     Объединяет функциональность <see cref="Serializer"/>, <see cref="CrystalCache"/>,
    ///     а также весь экспорт/импорт из <see cref="DataExporter" />.
    ///     Используйте один экземпляр этого класса вместо четырёх разрозненных файлов.
    /// </summary>
    public class DataIoService
    {
        #region ========  C A C H E  =================

        /// <summary>
        ///     Возвращает путь к XML‑кешу кристаллов для заданных параметров пластины.
        /// </summary>
        public string GetCacheFilePath(float sizeX, float sizeY, float diameter, float angleDeg = 0)
        {
            string safeFileName = angleDeg == 0
                ? $"Crystals_{sizeX}_{sizeY}_{diameter}.xml"
                : $"Crystals_{sizeX}_{sizeY}_{diameter}_{angleDeg:F1}.xml";

            return Path.Combine(GetStorageDirectory(), safeFileName);
        }

        /// <summary>
        ///     Сохраняет информацию о пластине и список кристаллов в XML‑кеш.
        /// </summary>
        public void SaveCache(string path, WaferInfo waferInfo, List<Crystal> crystals)
        {
            var serializer = new XmlSerializer(typeof(CacheData));
            using var writer = new StreamWriter(path);
            serializer.Serialize(writer, new CacheData { WaferInfo = waferInfo, Crystals = crystals });
        }

        /// <summary>
        ///     Пытается загрузить кристаллы из кеша.
        /// </summary>
        public bool TryLoadCache(string path, out WaferInfo waferInfo, out List<Crystal> crystals)
        {
            waferInfo = null;
            crystals = null;

            if (!File.Exists(path))
                return false;

            var serializer = new XmlSerializer(typeof(CacheData));
            try
            {
                using var reader = new StreamReader(path);
                var data = (CacheData)serializer.Deserialize(reader);
                waferInfo = data.WaferInfo;
                crystals = data.Crystals ?? new List<Crystal>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ========  W A F E R   I N F O  ========

        /// <summary>
        ///     Сериализует <see cref="WaferInfo"/> в файл <c>Stored data/WaferInfo.xml</c> внутри рабочей директории.
        ///     Совместимый переработанный вариант метода <c>Serializer.Serialize</c>.
        /// </summary>
        public void SerializeWaferInfo(WaferInfo waferInfo)
        {
            var xmlSerializer = new XmlSerializer(typeof(WaferInfo));
            using var writer = new StreamWriter(Path.Combine(GetStorageDirectory(), "WaferInfo.xml"));
            xmlSerializer.Serialize(writer, waferInfo);
        }

        #endregion

        #region ========  E X P O R T  ================

        public void ExportToCompactXml(string filePath, WaferInfo info, List<Crystal> crystals)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using var writer = XmlWriter.Create(filePath, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("WaferData");
            writer.WriteAttributeString("version", "1.0");

            // wafer info
            writer.WriteStartElement("WaferInfo");
            writer.WriteAttributeString("diameter", info.WaferDiameter.ToString());
            writer.WriteAttributeString("crystalWidth", info.SizeX.ToString());
            writer.WriteAttributeString("crystalHeight", info.SizeY.ToString());
            writer.WriteAttributeString("unit", "µm");
            writer.WriteEndElement();

            // crystals
            writer.WriteStartElement("Crystals");
            writer.WriteAttributeString("count", crystals.Count.ToString());
            foreach (var c in crystals)
            {
                writer.WriteStartElement("C");
                writer.WriteAttributeString("i", c.Index.ToString());
                writer.WriteAttributeString("x", c.RealX.ToString("F3"));
                writer.WriteAttributeString("y", c.RealY.ToString("F3"));
                if (c.Color != System.Drawing.Color.Blue)
                    writer.WriteAttributeString("color", c.Color.ToArgb().ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement(); // Crystals
            writer.WriteEndElement(); // WaferData
            writer.WriteEndDocument();
        }

        public void ExportToDetailedXml(string filePath, WaferInfo info, List<Crystal> crystals, WaferStatistics stats = null)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Replace
            };

            using var writer = XmlWriter.Create(filePath, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("WaferReport");
            writer.WriteAttributeString("xmlns", "http://crystaltable.com/schema/v1");
            writer.WriteAttributeString("generated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Metadata
            writer.WriteStartElement("Metadata");
            writer.WriteElementString("ExportVersion", "1.0");
            writer.WriteElementString("Application", "CrystalTable");
            writer.WriteElementString("ApplicationVersion", typeof(DataIoService).Assembly.GetName().Version.ToString());
            writer.WriteElementString("ExportDate", DateTime.Now.ToString("yyyy-MM-dd"));
            writer.WriteElementString("ExportTime", DateTime.Now.ToString("HH:mm:ss"));
            writer.WriteEndElement();

            // Wafer specification
            writer.WriteStartElement("WaferSpecification");
            writer.WriteElementString("Diameter", info.WaferDiameter.ToString());
            writer.WriteElementString("DiameterUnit", "mm");
            writer.WriteElementString("CrystalWidth", info.SizeX.ToString());
            writer.WriteElementString("CrystalHeight", info.SizeY.ToString());
            writer.WriteElementString("CrystalSizeUnit", "µm");
            writer.WriteElementString("WaferArea", (Math.PI * Math.Pow(info.WaferDiameter / 2.0, 2)).ToString("F2"));
            writer.WriteElementString("AreaUnit", "mm²");
            writer.WriteEndElement();

            // Statistics
            if (stats != null)
            {
                writer.WriteStartElement("Statistics");
                writer.WriteElementString("TotalCrystals", crystals.Count.ToString());
                writer.WriteElementString("FillPercentage", stats.CalculateFillPercentage(info.SizeX / 1000f, info.SizeY / 1000f).ToString("F2"));
                writer.WriteElementString("CrystalDensity", stats.GetCrystalDensity().ToString("F4"));

                // Quadrant distribution
                writer.WriteStartElement("QuadrantDistribution");
                foreach (var kvp in stats.GetQuadrantDistribution())
                {
                    writer.WriteStartElement("Quadrant");
                    writer.WriteAttributeString("name", kvp.Key);
                    writer.WriteAttributeString("count", kvp.Value.ToString());
                    writer.WriteAttributeString("percentage", ((float)kvp.Value / crystals.Count * 100).ToString("F1"));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // Radial distribution
                writer.WriteStartElement("RadialDistribution");
                foreach (var kvp in stats.GetRadialDistribution(5))
                {
                    writer.WriteStartElement("Ring");
                    writer.WriteAttributeString("radius", kvp.Key.ToString("F1"));
                    writer.WriteAttributeString("count", kvp.Value.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // Center of mass
                var com = stats.GetCenterOfMass();
                writer.WriteStartElement("CenterOfMass");
                writer.WriteAttributeString("x", com.X.ToString("F3"));
                writer.WriteAttributeString("y", com.Y.ToString("F3"));
                writer.WriteEndElement();

                writer.WriteEndElement(); // Statistics
            }

            // Crystal list
            writer.WriteStartElement("CrystalList");
            writer.WriteAttributeString("totalCount", crystals.Count.ToString());
            foreach (var c in crystals)
            {
                writer.WriteStartElement("Crystal");
                writer.WriteAttributeString("id", c.Index.ToString());

                writer.WriteStartElement("Position");
                writer.WriteAttributeString("x", c.RealX.ToString("F3"));
                writer.WriteAttributeString("y", c.RealY.ToString("F3"));
                writer.WriteAttributeString("unit", "mm");
                writer.WriteEndElement();

                writer.WriteStartElement("Properties");
                writer.WriteElementString("Color", c.Color.Name);
                writer.WriteElementString("ColorARGB", c.Color.ToArgb().ToString());

                float distance = (float)Math.Sqrt(c.RealX * c.RealX + c.RealY * c.RealY);
                writer.WriteElementString("DistanceFromCenter", distance.ToString("F3"));

                float angle = (float)(Math.Atan2(c.RealY, c.RealX) * 180 / Math.PI);
                writer.WriteElementString("AngleFromCenter", angle.ToString("F1"));
                writer.WriteEndElement(); // Properties

                writer.WriteEndElement(); // Crystal
            }
            writer.WriteEndElement(); // CrystalList
            writer.WriteEndElement(); // WaferReport
            writer.WriteEndDocument();
        }

        public void ExportToCsv(string filePath, List<Crystal> crystals, WaferInfo info = null)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            if (info != null)
            {
                writer.WriteLine($"# Wafer Diameter: {info.WaferDiameter} mm");
                writer.WriteLine($"# Crystal Size: {info.SizeX} x {info.SizeY} µm");
                writer.WriteLine($"# Total Crystals: {crystals.Count}");
                writer.WriteLine($"# Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();
            }

            writer.WriteLine("Index,X_Position_mm,Y_Position_mm,Distance_from_Center_mm,Angle_deg,Quadrant,Color");
            foreach (var c in crystals)
            {
                float distance = (float)Math.Sqrt(c.RealX * c.RealX + c.RealY * c.RealY);
                float angle = (float)(Math.Atan2(c.RealY, c.RealX) * 180 / Math.PI);
                string quadrant = c.RealX >= 0 && c.RealY >= 0 ? "Q1" :
                                  c.RealX < 0 && c.RealY >= 0 ? "Q2" :
                                  c.RealX < 0 && c.RealY < 0 ? "Q3" : "Q4";

                writer.WriteLine($"{c.Index},{c.RealX:F3},{c.RealY:F3},{distance:F3},{angle:F1},{quadrant},{c.Color.Name}");
            }
        }

        public void ExportToJson(string filePath, WaferInfo info, List<Crystal> crystals)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine("{");
            writer.WriteLine("  \"waferInfo\": {");
            writer.WriteLine($"    \"diameter\": {info.WaferDiameter},");
            writer.WriteLine($"    \"crystalWidth\": {info.SizeX},");
            writer.WriteLine($"    \"crystalHeight\": {info.SizeY},");
            writer.WriteLine("    \"units\": { \"diameter\": \"mm\", \"crystalSize\": \"µm\" }");
            writer.WriteLine("  },");
            writer.WriteLine("  \"metadata\": {");
            writer.WriteLine($"    \"exportDate\": \"{DateTime.Now:yyyy-MM-dd}\",");
            writer.WriteLine($"    \"exportTime\": \"{DateTime.Now:HH:mm:ss}\",");
            writer.WriteLine($"    \"totalCrystals\": {crystals.Count}");
            writer.WriteLine("  },");
            writer.WriteLine("  \"crystals\": [");
            for (int i = 0; i < crystals.Count; i++)
            {
                var c = crystals[i];
                writer.Write("    {");
                writer.Write($"\"index\": {c.Index}, \"x\": {c.RealX:F3}, \"y\": {c.RealY:F3}, \"color\": \"{c.Color.Name}\"}");
                writer.WriteLine(i < crystals.Count - 1 ? "," : string.Empty);
            }
            writer.WriteLine("  ]");
            writer.WriteLine("}");
        }

        #endregion

        #region ========  I M P O R T  ================

        public (WaferInfo info, List<Crystal> crystals) ImportFromCompactXml(string filePath)
        {
            var info = new WaferInfo();
            var crystals = new List<Crystal>();

            using var reader = XmlReader.Create(filePath);
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element) continue;

                switch (reader.Name)
                {
                    case "WaferInfo":
                        info.WaferDiameter = uint.Parse(reader.GetAttribute("diameter"));
                        info.SizeX = uint.Parse(reader.GetAttribute("crystalWidth"));
                        info.SizeY = uint.Parse(reader.GetAttribute("crystalHeight"));
                        break;
                    case "C":
                        var c = new Crystal
                        {
                            Index = int.Parse(reader.GetAttribute("i")),
                            RealX = float.Parse(reader.GetAttribute("x")),
                            RealY = float.Parse(reader.GetAttribute("y")),
                            Color = reader.GetAttribute("color") is { } colAttr
                                ? System.Drawing.Color.FromArgb(int.Parse(colAttr))
                                : System.Drawing.Color.Blue
                        };
                        crystals.Add(c);
                        break;
                }
            }
            return (info, crystals);
        }

        public List<Crystal> ImportFromCsv(string filePath)
        {
            var crystals = new List<Crystal>();
            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.Contains("Index,"))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                var c = new Crystal
                {
                    Index = int.Parse(parts[0]),
                    RealX = float.Parse(parts[1]),
                    RealY = float.Parse(parts[2]),
                    Color = parts.Length >= 7 && !string.IsNullOrEmpty(parts[6])
                        ? System.Drawing.Color.FromName(parts[6])
                        : System.Drawing.Color.Blue
                };
                crystals.Add(c);
            }
            return crystals;
        }

        #endregion

        #region ========  H E L P E R S  ==============

        public ExportFormat GetFormatFromExtension(string filePath)
        {
            return Path.GetExtension(filePath).ToLower() switch
            {
                ".xml" => ExportFormat.Xml,
                ".csv" => ExportFormat.Csv,
                ".json" => ExportFormat.Json,
                _ => ExportFormat.Unknown
            };
        }

        private static string GetStorageDirectory()
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Stored data");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        #endregion

        #region ========  N E S T E D   T Y P E S =====

        [Serializable]
        public class CacheData
        {
            public WaferInfo WaferInfo { get; set; }
            public List<Crystal> Crystals { get; set; }
        }

        #endregion
    }

    /// <summary>
    ///     Допустимые форматы экспорта.
    /// </summary>
    public enum ExportFormat
    {
        Unknown,
        Xml,
        Csv,
        Json
    }
}
