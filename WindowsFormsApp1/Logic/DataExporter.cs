using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Linq;
using System.Globalization;
using CrystalTable;
using CrystalTable.Data;
namespace CrystalTable.Logic
{
    /// <summary>
    /// Класс для экспорта и импорта данных в различные форматы
    /// </summary>
    public class DataExporter
    {
        /// <summary>
        /// Экспортирует данные в компактный XML формат
        /// </summary>
        public void ExportToCompactXml(string filePath, WaferInfo info, List<Crystal> crystals)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("WaferData");
                writer.WriteAttributeString("version", "1.0");

                // Информация о пластине в атрибутах для компактности
                writer.WriteStartElement("WaferInfo");
                writer.WriteAttributeString("diameter", info.WaferDiameter.ToString());
                writer.WriteAttributeString("crystalWidth", info.SizeX.ToString());
                writer.WriteAttributeString("crystalHeight", info.SizeY.ToString());
                writer.WriteAttributeString("unit", "µm");
                writer.WriteEndElement();

                // Кристаллы в компактном формате
                writer.WriteStartElement("Crystals");
                writer.WriteAttributeString("count", crystals.Count.ToString());

                foreach (var crystal in crystals)
                {
                    writer.WriteStartElement("C");
                    writer.WriteAttributeString("i", crystal.Index.ToString());
                    writer.WriteAttributeString("x", crystal.RealX.ToString("F3", CultureSettings.NumericCulture));
                    writer.WriteAttributeString("y", crystal.RealY.ToString("F3", CultureSettings.NumericCulture));
                    if (crystal.Color != System.Drawing.Color.Blue) // Сохраняем цвет только если не стандартный
                    {
                        writer.WriteAttributeString("color", crystal.Color.ToArgb().ToString());
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); // Crystals
                writer.WriteEndElement(); // WaferData
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Экспортирует данные в детальный XML формат с дополнительной информацией
        /// </summary>
        public void ExportToDetailedXml(string filePath, WaferInfo info,
            List<Crystal> crystals, WaferStatistics stats = null)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Replace
            };

            using (var writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("WaferReport");
                writer.WriteAttributeString("xmlns", "http://crystaltable.com/schema/v1");
                writer.WriteAttributeString("generated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                // Метаданные
                writer.WriteStartElement("Metadata");
                writer.WriteElementString("ExportVersion", "1.0");
                writer.WriteElementString("Application", "CrystalTable");
                writer.WriteElementString("ApplicationVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                writer.WriteElementString("ExportDate", DateTime.Now.ToString("yyyy-MM-dd"));
                writer.WriteElementString("ExportTime", DateTime.Now.ToString("HH:mm:ss"));
                writer.WriteEndElement();

                // Информация о пластине
                writer.WriteStartElement("WaferSpecification");
                writer.WriteElementString("Diameter", info.WaferDiameter.ToString());
                writer.WriteElementString("DiameterUnit", "mm");
                writer.WriteElementString("CrystalWidth", info.SizeX.ToString());
                writer.WriteElementString("CrystalHeight", info.SizeY.ToString());
                writer.WriteElementString("CrystalSizeUnit", "µm");
                writer.WriteElementString("StepXmm", info.StepXmm.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("StepYmm", info.StepYmm.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("RotationDeg", info.RotationAngleDeg.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("Zoom", info.ZoomFactor.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("WaferArea", (Math.PI * Math.Pow(info.WaferDiameter / 2, 2)).ToString("F2", CultureSettings.NumericCulture));
                writer.WriteElementString("AreaUnit", "mm2");
                writer.WriteEndElement();

                // Статистика (если предоставлена)
                if (stats != null)
                {
                    writer.WriteStartElement("Statistics");
                    writer.WriteElementString("TotalCrystals", crystals.Count.ToString());
                    writer.WriteElementString("FillPercentage",
                        stats.CalculateFillPercentage(info.SizeX / 1000f, info.SizeY / 1000f).ToString("F2", CultureSettings.NumericCulture));
                    writer.WriteElementString("CrystalDensity",
                        stats.GetCrystalDensity().ToString("F4", CultureSettings.NumericCulture));

                    // Распределение по квадрантам
                    writer.WriteStartElement("QuadrantDistribution");
                    foreach (var kvp in stats.GetQuadrantDistribution())
                    {
                        writer.WriteStartElement("Quadrant");
                        writer.WriteAttributeString("name", kvp.Key);
                        writer.WriteAttributeString("count", kvp.Value.ToString());
                        writer.WriteAttributeString("percentage",
                            ((float)kvp.Value / crystals.Count * 100).ToString("F1", CultureSettings.NumericCulture));
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // Распределение по радиусу
                    writer.WriteStartElement("RadialDistribution");
                    var radialDist = stats.GetRadialDistribution(5);
                    foreach (var kvp in radialDist)
                    {
                        writer.WriteStartElement("Ring");
                        writer.WriteAttributeString("radius", kvp.Key.ToString("F1", CultureSettings.NumericCulture));
                        writer.WriteAttributeString("count", kvp.Value.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // Центр масс
                    var centerOfMass = stats.GetCenterOfMass();
                    writer.WriteStartElement("CenterOfMass");
                    writer.WriteAttributeString("x", centerOfMass.X.ToString("F3", CultureSettings.NumericCulture));
                    writer.WriteAttributeString("y", centerOfMass.Y.ToString("F3", CultureSettings.NumericCulture));
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // Statistics
                }

                // Детальная информация о кристаллах
                writer.WriteStartElement("CrystalList");
                writer.WriteAttributeString("totalCount", crystals.Count.ToString());

                foreach (var crystal in crystals)
                {
                    writer.WriteStartElement("Crystal");
                    writer.WriteAttributeString("id", crystal.Index.ToString());

                    writer.WriteStartElement("Position");
                    writer.WriteAttributeString("x", crystal.RealX.ToString("F3", CultureSettings.NumericCulture));
                    writer.WriteAttributeString("y", crystal.RealY.ToString("F3", CultureSettings.NumericCulture));
                    writer.WriteAttributeString("unit", "mm");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Properties");
                    writer.WriteElementString("Color", crystal.Color.Name);
                    writer.WriteElementString("ColorARGB", crystal.Color.ToArgb().ToString());

                    // Расстояние от центра
                    float distance = (float)Math.Sqrt(crystal.RealX * crystal.RealX + crystal.RealY * crystal.RealY);
                    writer.WriteElementString("DistanceFromCenter", distance.ToString("F3", CultureSettings.NumericCulture));

                    // Угол относительно центра
                    float angle = (float)(Math.Atan2(crystal.RealY, crystal.RealX) * 180 / Math.PI);
                    writer.WriteElementString("AngleFromCenter", angle.ToString("F1", CultureSettings.NumericCulture));

                    writer.WriteEndElement(); // Properties
                    writer.WriteEndElement(); // Crystal
                }

                writer.WriteEndElement(); // CrystalList
                writer.WriteEndElement(); // WaferReport
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Экспортирует данные в CSV формат
        /// </summary>
        public void ExportToCsv(string filePath, List<Crystal> crystals, WaferInfo info = null)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                if (info != null)
                {
                    writer.WriteLine($"# Diameter_mm: {info.WaferDiameter.ToString(CultureSettings.NumericCulture)}");
                    writer.WriteLine($"# Crystal_um: {info.SizeX.ToString(CultureSettings.NumericCulture)} x {info.SizeY.ToString(CultureSettings.NumericCulture)}");
                    writer.WriteLine($"# Step_mm: {info.StepXmm.ToString("F3", CultureSettings.NumericCulture)} x {info.StepYmm.ToString("F3", CultureSettings.NumericCulture)}");
                    writer.WriteLine($"# Rotation_deg: {info.RotationAngleDeg.ToString("F3", CultureSettings.NumericCulture)}");
                    writer.WriteLine($"# Zoom: {info.ZoomFactor.ToString("F3", CultureSettings.NumericCulture)}");
                    writer.WriteLine();
                }

                writer.WriteLine("Index;X_mm;Y_mm;Distance_mm;Angle_deg;Quadrant;Color;VisualX;VisualY;VisualLeft;VisualTop;VisualRight;VisualBottom");

                foreach (var crystal in crystals)
                {
                    float distance = (float)Math.Sqrt(crystal.RealX * crystal.RealX + crystal.RealY * crystal.RealY);
                    float angle = (float)(Math.Atan2(crystal.RealY, crystal.RealX) * 180 / Math.PI);
                    string quadrant = DetermineQuadrant(crystal);

                    writer.WriteLine(string.Join(";", new[]
                    {
                        crystal.Index.ToString(),
                        crystal.RealX.ToString("F3", CultureSettings.NumericCulture),
                        crystal.RealY.ToString("F3", CultureSettings.NumericCulture),
                        distance.ToString("F3", CultureSettings.NumericCulture),
                        angle.ToString("F1", CultureSettings.NumericCulture),
                        quadrant,
                        crystal.Color.Name,
                        crystal.DisplayX.ToString("F3", CultureSettings.NumericCulture),
                        crystal.DisplayY.ToString("F3", CultureSettings.NumericCulture),
                        crystal.DisplayLeft.ToString("F3", CultureSettings.NumericCulture),
                        crystal.DisplayTop.ToString("F3", CultureSettings.NumericCulture),
                        crystal.DisplayRight.ToString("F3", CultureSettings.NumericCulture),
                        crystal.DisplayBottom.ToString("F3", CultureSettings.NumericCulture)
                    }));
                }
            }
        }        public void ExportToJson(string filePath, WaferInfo info, List<Crystal> crystals)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("{");
                writer.WriteLine("  \"waferInfo\": {");
                writer.WriteLine($"    \"diameter\": \"{info.WaferDiameter.ToString(CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"crystalWidth\": \"{info.SizeX.ToString(CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"crystalHeight\": \"{info.SizeY.ToString(CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"stepXmm\": \"{info.StepXmm.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"stepYmm\": \"{info.StepYmm.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"rotationDeg\": \"{info.RotationAngleDeg.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"zoom\": \"{info.ZoomFactor.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"panX\": \"{info.PanOffsetX.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"panY\": \"{info.PanOffsetY.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"pointerX\": \"{info.PointerXmm.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"pointerY\": \"{info.PointerYmm.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"calibrated\": \"{(info.HasCalibration ? "1" : "0")}\",");
                writer.WriteLine($"    \"firstRefX\": \"{info.FirstReferenceX.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"firstRefY\": \"{info.FirstReferenceY.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"lastRefX\": \"{info.LastReferenceX.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"    \"lastRefY\": \"{info.LastReferenceY.ToString("F3", CultureSettings.NumericCulture)}\"");
                writer.WriteLine("  },");
                writer.WriteLine("  \"metadata\": {");
                writer.WriteLine($"    \"exportDate\": \"{DateTime.Now:yyyy-MM-dd}\",");
                writer.WriteLine($"    \"exportTime\": \"{DateTime.Now:HH:mm:ss}\",");
                writer.WriteLine($"    \"totalCrystals\": \"{crystals.Count}\"");
                writer.WriteLine("  },");
                writer.WriteLine("  \"crystals\": [");

                for (int i = 0; i < crystals.Count; i++)
                {
                    var crystal = crystals[i];
                    float distance = (float)Math.Sqrt(crystal.RealX * crystal.RealX + crystal.RealY * crystal.RealY);
                    float angle = (float)(Math.Atan2(crystal.RealY, crystal.RealX) * 180 / Math.PI);
                    string quadrant = DetermineQuadrant(crystal);

                    writer.WriteLine("    {");
                    writer.WriteLine($"      \"index\": \"{crystal.Index}\",");
                    writer.WriteLine($"      \"x\": \"{crystal.RealX.ToString("F3", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"y\": \"{crystal.RealY.ToString("F3", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"distance\": \"{distance.ToString("F3", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"angle\": \"{angle.ToString("F1", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"quadrant\": \"{quadrant}\",");
                    writer.WriteLine($"      \"color\": \"{crystal.Color.Name}\",");
                    writer.WriteLine($"      \"visualX\": \"{crystal.DisplayX.ToString("F3", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"visualY\": \"{crystal.DisplayY.ToString("F3", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"visualLeft\": \"{crystal.DisplayLeft.ToString("F3", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"visualTop\": \"{crystal.DisplayTop.ToString("F3", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"visualRight\": \"{crystal.DisplayRight.ToString("F3", CultureSettings.NumericCulture)}\",");
                    writer.WriteLine($"      \"visualBottom\": \"{crystal.DisplayBottom.ToString("F3", CultureSettings.NumericCulture)}\"");
                    writer.Write("    }");
                    if (i < crystals.Count - 1)
                    {
                        writer.WriteLine(",");
                    }
                    else
                    {
                        writer.WriteLine();
                    }
                }

                writer.WriteLine("  ]");
                writer.WriteLine("}");
            }
        }        public (WaferInfo info, List<Crystal> crystals) ImportFromCompactXml(string filePath)
        {
            var info = new WaferInfo();
            var crystals = new List<Crystal>();

            using (var reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    switch (reader.Name)
                    {
                        case "WaferInfo":
                            info.WaferDiameter = ParseUInt(reader.GetAttribute("diameter"));
                            info.SizeX = ParseUInt(reader.GetAttribute("crystalWidth"));
                            info.SizeY = ParseUInt(reader.GetAttribute("crystalHeight"));
                            info.StepXmm = ParseFloat(reader.GetAttribute("stepXmm"), info.StepXmm);
                            info.StepYmm = ParseFloat(reader.GetAttribute("stepYmm"), info.StepYmm);
                            info.RotationAngleDeg = ParseFloat(reader.GetAttribute("rotationDeg"), info.RotationAngleDeg);
                            info.ZoomFactor = ParseFloat(reader.GetAttribute("zoom"), 1f);
                            info.PanOffsetX = ParseFloat(reader.GetAttribute("panX"), info.PanOffsetX);
                            info.PanOffsetY = ParseFloat(reader.GetAttribute("panY"), info.PanOffsetY);
                            info.PointerXmm = ParseFloat(reader.GetAttribute("pointerX"), info.PointerXmm);
                            info.PointerYmm = ParseFloat(reader.GetAttribute("pointerY"), info.PointerYmm);

                            string calibrated = reader.GetAttribute("calibrated");
                            info.HasCalibration = calibrated == "1" || string.Equals(calibrated, "true", StringComparison.OrdinalIgnoreCase);
                            if (info.HasCalibration)
                            {
                                info.FirstReferenceX = ParseFloat(reader.GetAttribute("firstRefX"), info.FirstReferenceX);
                                info.FirstReferenceY = ParseFloat(reader.GetAttribute("firstRefY"), info.FirstReferenceY);
                                info.LastReferenceX = ParseFloat(reader.GetAttribute("lastRefX"), info.LastReferenceX);
                                info.LastReferenceY = ParseFloat(reader.GetAttribute("lastRefY"), info.LastReferenceY);
                            }
                            break;

                        case "C":
                            var crystal = new Crystal
                            {
                                Index = int.Parse(reader.GetAttribute("i")),
                                RealX = ParseFloat(reader.GetAttribute("x")),
                                RealY = ParseFloat(reader.GetAttribute("y")),
                                Color = System.Drawing.Color.Blue
                            };

                            string colorAttr = reader.GetAttribute("color");
                            if (!string.IsNullOrEmpty(colorAttr))
                            {
                                crystal.Color = System.Drawing.Color.FromArgb(int.Parse(colorAttr));
                            }

                            crystal.DisplayX = ParseFloat(reader.GetAttribute("vx"), crystal.DisplayX);
                            crystal.DisplayY = ParseFloat(reader.GetAttribute("vy"), crystal.DisplayY);
                            crystal.DisplayLeft = ParseFloat(reader.GetAttribute("vleft"), crystal.DisplayLeft);
                            crystal.DisplayTop = ParseFloat(reader.GetAttribute("vtop"), crystal.DisplayTop);
                            crystal.DisplayRight = ParseFloat(reader.GetAttribute("vright"), crystal.DisplayRight);
                            crystal.DisplayBottom = ParseFloat(reader.GetAttribute("vbottom"), crystal.DisplayBottom);

                            crystals.Add(crystal);
                            break;
                    }
                }
            }

            return (info, crystals);
        }/// <summary>
        /// Импортирует данные из CSV файла
        /// </summary>
                public List<Crystal> ImportFromCsv(string filePath)
        {
            var crystals = new List<Crystal>();
            var lines = File.ReadAllLines(filePath);

            foreach (var rawLine in lines)
            {
                if (string.IsNullOrWhiteSpace(rawLine) || rawLine.StartsWith("#"))
                {
                    continue;
                }

                var line = rawLine.Trim();
                if (line.StartsWith("Index", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string[] parts = line.Split(';');
                if (parts.Length < 3)
                {
                    parts = line.Split(',');
                }

                if (parts.Length < 3)
                {
                    continue;
                }

                var crystal = new Crystal
                {
                    Index = int.Parse(parts[0]),
                    RealX = ParseFloat(parts[1]),
                    RealY = ParseFloat(parts[2]),
                    Color = System.Drawing.Color.Blue
                };

                if (parts.Length > 6 && !string.IsNullOrEmpty(parts[6]))
                {
                    try
                    {
                        crystal.Color = System.Drawing.Color.FromName(parts[6]);
                    }
                    catch
                    {
                        crystal.Color = System.Drawing.Color.Blue;
                    }
                }

                if (parts.Length > 7)
                {
                    crystal.DisplayX = ParseFloat(parts[7], crystal.DisplayX);
                }

                if (parts.Length > 8)
                {
                    crystal.DisplayY = ParseFloat(parts[8], crystal.DisplayY);
                }

                if (parts.Length > 9)
                {
                    crystal.DisplayLeft = ParseFloat(parts[9], crystal.DisplayLeft);
                }

                if (parts.Length > 10)
                {
                    crystal.DisplayTop = ParseFloat(parts[10], crystal.DisplayTop);
                }

                if (parts.Length > 11)
                {
                    crystal.DisplayRight = ParseFloat(parts[11], crystal.DisplayRight);
                }

                if (parts.Length > 12)
                {
                    crystal.DisplayBottom = ParseFloat(parts[12], crystal.DisplayBottom);
                }

                crystals.Add(crystal);
            }

            return crystals;
        }

        /// <summary>
        /// Определяет формат файла по расширению
        /// </summary>
        private static string DetermineQuadrant(Crystal crystal)
        {
            if (crystal.RealX >= 0 && crystal.RealY >= 0)
            {
                return "Q1";
            }

            if (crystal.RealX < 0 && crystal.RealY >= 0)
            {
                return "Q2";
            }

            if (crystal.RealX < 0 && crystal.RealY < 0)
            {
                return "Q3";
            }

            return "Q4";
        }
        private static float ParseFloat(string source, float fallback = 0f)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return fallback;
            }

            return float.TryParse(source, NumberStyles.Float, CultureSettings.NumericCulture, out var value)
                ? value
                : fallback;
        }

        private static uint ParseUInt(string source, uint fallback = 0)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return fallback;
            }

            return uint.TryParse(source, NumberStyles.Integer, CultureSettings.NumericCulture, out var value)
                ? value
                : fallback;
        }
        public ExportFormat GetFormatFromExtension(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".xml":
                    return ExportFormat.Xml;
                case ".csv":
                    return ExportFormat.Csv;
                case ".json":
                    return ExportFormat.Json;
                default:
                    return ExportFormat.Unknown;
            }
        }
    }

    /// <summary>
    /// Форматы экспорта
    /// </summary>
    public enum ExportFormat
    {
        Unknown,
        Xml,
        Csv,
        Json
    }
}












