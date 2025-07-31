using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Linq;
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
                    writer.WriteAttributeString("x", crystal.RealX.ToString("F3"));
                    writer.WriteAttributeString("y", crystal.RealY.ToString("F3"));
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
                writer.WriteElementString("WaferArea", (Math.PI * Math.Pow(info.WaferDiameter / 2, 2)).ToString("F2"));
                writer.WriteElementString("AreaUnit", "mm²");
                writer.WriteEndElement();

                // Статистика (если предоставлена)
                if (stats != null)
                {
                    writer.WriteStartElement("Statistics");
                    writer.WriteElementString("TotalCrystals", crystals.Count.ToString());
                    writer.WriteElementString("FillPercentage",
                        stats.CalculateFillPercentage(info.SizeX / 1000f, info.SizeY / 1000f).ToString("F2"));
                    writer.WriteElementString("CrystalDensity",
                        stats.GetCrystalDensity().ToString("F4"));

                    // Распределение по квадрантам
                    writer.WriteStartElement("QuadrantDistribution");
                    foreach (var kvp in stats.GetQuadrantDistribution())
                    {
                        writer.WriteStartElement("Quadrant");
                        writer.WriteAttributeString("name", kvp.Key);
                        writer.WriteAttributeString("count", kvp.Value.ToString());
                        writer.WriteAttributeString("percentage",
                            ((float)kvp.Value / crystals.Count * 100).ToString("F1"));
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // Распределение по радиусу
                    writer.WriteStartElement("RadialDistribution");
                    var radialDist = stats.GetRadialDistribution(5);
                    foreach (var kvp in radialDist)
                    {
                        writer.WriteStartElement("Ring");
                        writer.WriteAttributeString("radius", kvp.Key.ToString("F1"));
                        writer.WriteAttributeString("count", kvp.Value.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // Центр масс
                    var centerOfMass = stats.GetCenterOfMass();
                    writer.WriteStartElement("CenterOfMass");
                    writer.WriteAttributeString("x", centerOfMass.X.ToString("F3"));
                    writer.WriteAttributeString("y", centerOfMass.Y.ToString("F3"));
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
                    writer.WriteAttributeString("x", crystal.RealX.ToString("F3"));
                    writer.WriteAttributeString("y", crystal.RealY.ToString("F3"));
                    writer.WriteAttributeString("unit", "mm");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Properties");
                    writer.WriteElementString("Color", crystal.Color.Name);
                    writer.WriteElementString("ColorARGB", crystal.Color.ToArgb().ToString());

                    // Расстояние от центра
                    float distance = (float)Math.Sqrt(crystal.RealX * crystal.RealX + crystal.RealY * crystal.RealY);
                    writer.WriteElementString("DistanceFromCenter", distance.ToString("F3"));

                    // Угол относительно центра
                    float angle = (float)(Math.Atan2(crystal.RealY, crystal.RealX) * 180 / Math.PI);
                    writer.WriteElementString("AngleFromCenter", angle.ToString("F1"));

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
                // Заголовок с информацией о пластине
                if (info != null)
                {
                    writer.WriteLine($"# Wafer Diameter: {info.WaferDiameter} mm");
                    writer.WriteLine($"# Crystal Size: {info.SizeX} x {info.SizeY} µm");
                    writer.WriteLine($"# Total Crystals: {crystals.Count}");
                    writer.WriteLine($"# Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine();
                }

                // Заголовки колонок
                writer.WriteLine("Index,X_Position_mm,Y_Position_mm,Distance_from_Center_mm,Angle_deg,Quadrant,Color");

                // Данные
                foreach (var crystal in crystals)
                {
                    float distance = (float)Math.Sqrt(crystal.RealX * crystal.RealX +
                                                     crystal.RealY * crystal.RealY);
                    float angle = (float)(Math.Atan2(crystal.RealY, crystal.RealX) * 180 / Math.PI);

                    // Определяем квадрант
                    string quadrant;
                    if (crystal.RealX >= 0 && crystal.RealY >= 0) quadrant = "Q1";
                    else if (crystal.RealX < 0 && crystal.RealY >= 0) quadrant = "Q2";
                    else if (crystal.RealX < 0 && crystal.RealY < 0) quadrant = "Q3";
                    else quadrant = "Q4";

                    writer.WriteLine($"{crystal.Index},{crystal.RealX:F3},{crystal.RealY:F3}," +
                                   $"{distance:F3},{angle:F1},{quadrant},{crystal.Color.Name}");
                }
            }
        }

        /// <summary>
        /// Экспортирует данные в формат JSON
        /// </summary>
        public void ExportToJson(string filePath, WaferInfo info, List<Crystal> crystals)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("{");

                // Информация о пластине
                writer.WriteLine("  \"waferInfo\": {");
                writer.WriteLine($"    \"diameter\": {info.WaferDiameter},");
                writer.WriteLine($"    \"crystalWidth\": {info.SizeX},");
                writer.WriteLine($"    \"crystalHeight\": {info.SizeY},");
                writer.WriteLine("    \"units\": {");
                writer.WriteLine("      \"diameter\": \"mm\",");
                writer.WriteLine("      \"crystalSize\": \"µm\"");
                writer.WriteLine("    }");
                writer.WriteLine("  },");

                // Метаданные
                writer.WriteLine("  \"metadata\": {");
                writer.WriteLine($"    \"exportDate\": \"{DateTime.Now:yyyy-MM-dd}\",");
                writer.WriteLine($"    \"exportTime\": \"{DateTime.Now:HH:mm:ss}\",");
                writer.WriteLine($"    \"totalCrystals\": {crystals.Count}");
                writer.WriteLine("  },");

                // Кристаллы
                writer.WriteLine("  \"crystals\": [");

                for (int i = 0; i < crystals.Count; i++)
                {
                    var crystal = crystals[i];
                    writer.Write("    {");
                    writer.Write($"\"index\": {crystal.Index}, ");
                    writer.Write($"\"x\": {crystal.RealX:F3}, ");
                    writer.Write($"\"y\": {crystal.RealY:F3}, ");
                    writer.Write($"\"color\": \"{crystal.Color.Name}\"");
                    writer.Write("}");

                    if (i < crystals.Count - 1)
                        writer.WriteLine(",");
                    else
                        writer.WriteLine();
                }

                writer.WriteLine("  ]");
                writer.WriteLine("}");
            }
        }

        /// <summary>
        /// Импортирует данные из компактного XML
        /// </summary>
        public (WaferInfo info, List<Crystal> crystals) ImportFromCompactXml(string filePath)
        {
            var info = new WaferInfo();
            var crystals = new List<Crystal>();

            using (var reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "WaferInfo":
                                info.WaferDiameter = uint.Parse(reader.GetAttribute("diameter"));
                                info.SizeX = uint.Parse(reader.GetAttribute("crystalWidth"));
                                info.SizeY = uint.Parse(reader.GetAttribute("crystalHeight"));
                                break;

                            case "C": // Crystal
                                var crystal = new Crystal
                                {
                                    Index = int.Parse(reader.GetAttribute("i")),
                                    RealX = float.Parse(reader.GetAttribute("x")),
                                    RealY = float.Parse(reader.GetAttribute("y"))
                                };

                                string colorAttr = reader.GetAttribute("color");
                                if (!string.IsNullOrEmpty(colorAttr))
                                {
                                    crystal.Color = System.Drawing.Color.FromArgb(int.Parse(colorAttr));
                                }
                                else
                                {
                                    crystal.Color = System.Drawing.Color.Blue;
                                }

                                crystals.Add(crystal);
                                break;
                        }
                    }
                }
            }

            return (info, crystals);
        }

        /// <summary>
        /// Импортирует данные из CSV файла
        /// </summary>
        public List<Crystal> ImportFromCsv(string filePath)
        {
            var crystals = new List<Crystal>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // Пропускаем комментарии и заголовки
                if (line.StartsWith("#") || line.Contains("Index,") || string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    var crystal = new Crystal
                    {
                        Index = int.Parse(parts[0]),
                        RealX = float.Parse(parts[1]),
                        RealY = float.Parse(parts[2])
                    };

                    // Цвет (если есть)
                    if (parts.Length >= 7 && !string.IsNullOrEmpty(parts[6]))
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
                    else
                    {
                        crystal.Color = System.Drawing.Color.Blue;
                    }

                    crystals.Add(crystal);
                }
            }

            return crystals;
        }

        /// <summary>
        /// Определяет формат файла по расширению
        /// </summary>
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