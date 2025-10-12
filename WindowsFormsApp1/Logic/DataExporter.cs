using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CrystalTable;
using CrystalTable.Data;

namespace CrystalTable.Logic
{
    /// <summary>
    /// Экспорт и импорт данных карты пластины.
    /// </summary>
    public class DataExporter
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

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
            writer.WriteAttributeString("version", "2.0");

            writer.WriteStartElement("WaferInfo");
            writer.WriteAttributeString("diameter", info.WaferDiameter.ToString(CultureSettings.NumericCulture));
            writer.WriteAttributeString("crystalWidthUm", info.SizeX.ToString(CultureSettings.NumericCulture));
            writer.WriteAttributeString("crystalHeightUm", info.SizeY.ToString(CultureSettings.NumericCulture));
            writer.WriteAttributeString("streetMm", info.StreetMm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("offsetXmm", info.OffsetXMm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("offsetYmm", info.OffsetYMm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("mirrorX", info.MirrorX ? "1" : "0");
            writer.WriteAttributeString("mirrorY", info.MirrorY ? "1" : "0");
            writer.WriteAttributeString("swap", info.OrientationSwapped ? "1" : "0");
            writer.WriteAttributeString("edgeExclusionMm", info.EdgeExclusionMm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("stepXmm", info.StepXmm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("stepYmm", info.StepYmm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("rotationDeg", info.RotationAngleDeg.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("zoom", info.ZoomFactor.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("panX", info.PanOffsetX.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("panY", info.PanOffsetY.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("pointerX", info.PointerXmm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("pointerY", info.PointerYmm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteAttributeString("calibrated", info.HasCalibration ? "1" : "0");
            if (info.HasCalibration)
            {
                writer.WriteAttributeString("firstRefX", info.FirstReferenceX.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("firstRefY", info.FirstReferenceY.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("lastRefX", info.LastReferenceX.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("lastRefY", info.LastReferenceY.ToString("F3", CultureSettings.NumericCulture));
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Crystals");
            writer.WriteAttributeString("count", crystals.Count.ToString(CultureInfo.InvariantCulture));

            foreach (var crystal in crystals)
            {
                writer.WriteStartElement("C");
                writer.WriteAttributeString("i", crystal.Index.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("x", crystal.RealX.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("y", crystal.RealY.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("w", crystal.WidthMm.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("h", crystal.HeightMm.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("status", crystal.PlacementStatus.ToString());
                if (crystal.Color != Color.Blue)
                {
                    writer.WriteAttributeString("color", crystal.Color.ToArgb().ToString(CultureInfo.InvariantCulture));
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
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
            writer.WriteAttributeString("xmlns", "http://crystaltable.com/schema/v2");
            writer.WriteAttributeString("generated", DateTime.Now.ToString(DateTimeFormat));

            writer.WriteStartElement("Metadata");
            writer.WriteElementString("ExportVersion", "2.0");
            writer.WriteElementString("Application", "CrystalTable");
            writer.WriteElementString("ApplicationVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            writer.WriteElementString("ExportDate", DateTime.Now.ToString("yyyy-MM-dd"));
            writer.WriteElementString("ExportTime", DateTime.Now.ToString("HH:mm:ss"));
            writer.WriteEndElement();

            writer.WriteStartElement("WaferSpecification");
            writer.WriteElementString("Diameter", info.WaferDiameter.ToString(CultureSettings.NumericCulture));
            writer.WriteElementString("DiameterUnit", "mm");
            writer.WriteElementString("CrystalWidthUm", info.SizeX.ToString(CultureSettings.NumericCulture));
            writer.WriteElementString("CrystalHeightUm", info.SizeY.ToString(CultureSettings.NumericCulture));
            writer.WriteElementString("StepXmm", info.StepXmm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("StepYmm", info.StepYmm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("StreetMm", info.StreetMm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("OffsetXMm", info.OffsetXMm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("OffsetYMm", info.OffsetYMm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("MirrorX", info.MirrorX ? "true" : "false");
            writer.WriteElementString("MirrorY", info.MirrorY ? "true" : "false");
            writer.WriteElementString("OrientationSwapped", info.OrientationSwapped ? "true" : "false");
            writer.WriteElementString("EdgeExclusionMm", info.EdgeExclusionMm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("RotationDeg", info.RotationAngleDeg.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteEndElement();

            writer.WriteStartElement("WaferState");
            writer.WriteElementString("Zoom", info.ZoomFactor.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("PanX", info.PanOffsetX.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("PanY", info.PanOffsetY.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("PointerX", info.PointerXmm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("PointerY", info.PointerYmm.ToString("F3", CultureSettings.NumericCulture));
            writer.WriteElementString("HasCalibration", info.HasCalibration ? "true" : "false");
            if (info.HasCalibration)
            {
                writer.WriteElementString("FirstRefX", info.FirstReferenceX.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("FirstRefY", info.FirstReferenceY.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("LastRefX", info.LastReferenceX.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("LastRefY", info.LastReferenceY.ToString("F3", CultureSettings.NumericCulture));
            }
            writer.WriteEndElement();

            writer.WriteStartElement("CrystalList");
            writer.WriteAttributeString("totalCount", crystals.Count.ToString(CultureInfo.InvariantCulture));

            foreach (var crystal in crystals)
            {
                writer.WriteStartElement("Crystal");
                writer.WriteAttributeString("id", crystal.Index.ToString(CultureInfo.InvariantCulture));

                writer.WriteStartElement("Position");
                writer.WriteAttributeString("x", crystal.RealX.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("y", crystal.RealY.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteAttributeString("unit", "mm");
                writer.WriteEndElement();

                writer.WriteStartElement("Geometry");
                writer.WriteElementString("WidthMm", crystal.WidthMm.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("HeightMm", crystal.HeightMm.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("Status", crystal.PlacementStatus.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("Visual");
                writer.WriteElementString("DisplayX", crystal.DisplayX.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("DisplayY", crystal.DisplayY.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("DisplayLeft", crystal.DisplayLeft.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("DisplayTop", crystal.DisplayTop.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("DisplayRight", crystal.DisplayRight.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteElementString("DisplayBottom", crystal.DisplayBottom.ToString("F3", CultureSettings.NumericCulture));
                writer.WriteEndElement();

                writer.WriteStartElement("Appearance");
                writer.WriteElementString("ColorName", crystal.Color.Name);
                writer.WriteElementString("ColorArgb", crystal.Color.ToArgb().ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();

                if (stats != null)
                {
                    float distance = (float)Math.Sqrt(crystal.RealX * crystal.RealX + crystal.RealY * crystal.RealY);
                    float angle = (float)(Math.Atan2(crystal.RealY, crystal.RealX) * 180.0 / Math.PI);
                    writer.WriteElementString("DistanceFromCenter", distance.ToString("F3", CultureSettings.NumericCulture));
                    writer.WriteElementString("AngleFromCenter", angle.ToString("F1", CultureSettings.NumericCulture));
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            if (stats != null)
            {
                writer.WriteStartElement("Statistics");
                int fullCount = crystals.Count(c => c.PlacementStatus == CrystalPlacementStatus.Full);
                int partialCount = crystals.Count(c => c.PlacementStatus == CrystalPlacementStatus.Partial);
                writer.WriteElementString("TotalCrystals", crystals.Count.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("FullCrystals", fullCount.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("PartialCrystals", partialCount.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public void ExportToCsv(string filePath, List<Crystal> crystals, WaferInfo info = null)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            if (info != null)
            {
                writer.WriteLine($"# Diameter_mm: {info.WaferDiameter.ToString(CultureSettings.NumericCulture)}");
                writer.WriteLine($"# Crystal_um: {info.SizeX.ToString(CultureSettings.NumericCulture)} x {info.SizeY.ToString(CultureSettings.NumericCulture)}");
                writer.WriteLine($"# Step_mm: {info.StepXmm.ToString("F3", CultureSettings.NumericCulture)} x {info.StepYmm.ToString("F3", CultureSettings.NumericCulture)}");
                writer.WriteLine($"# Street_mm: {info.StreetMm.ToString("F3", CultureSettings.NumericCulture)}");
                writer.WriteLine($"# Offset_mm: {info.OffsetXMm.ToString("F3", CultureSettings.NumericCulture)} x {info.OffsetYMm.ToString("F3", CultureSettings.NumericCulture)}");
                writer.WriteLine();
            }

            writer.WriteLine("Index;X_mm;Y_mm;Distance_mm;Angle_deg;Quadrant;Color;Width_mm;Height_mm;Status;VisualX;VisualY;VisualLeft;VisualTop;VisualRight;VisualBottom");

            foreach (var crystal in crystals)
            {
                float distance = (float)Math.Sqrt(crystal.RealX * crystal.RealX + crystal.RealY * crystal.RealY);
                float angle = (float)(Math.Atan2(crystal.RealY, crystal.RealX) * 180.0 / Math.PI);
                string quadrant = DetermineQuadrant(crystal);

                writer.WriteLine(string.Join(";", new[]
                {
                    crystal.Index.ToString(CultureInfo.InvariantCulture),
                    crystal.RealX.ToString("F3", CultureSettings.NumericCulture),
                    crystal.RealY.ToString("F3", CultureSettings.NumericCulture),
                    distance.ToString("F3", CultureSettings.NumericCulture),
                    angle.ToString("F1", CultureSettings.NumericCulture),
                    quadrant,
                    crystal.Color.Name,
                    crystal.WidthMm.ToString("F3", CultureSettings.NumericCulture),
                    crystal.HeightMm.ToString("F3", CultureSettings.NumericCulture),
                    crystal.PlacementStatus.ToString(),
                    crystal.DisplayX.ToString("F3", CultureSettings.NumericCulture),
                    crystal.DisplayY.ToString("F3", CultureSettings.NumericCulture),
                    crystal.DisplayLeft.ToString("F3", CultureSettings.NumericCulture),
                    crystal.DisplayTop.ToString("F3", CultureSettings.NumericCulture),
                    crystal.DisplayRight.ToString("F3", CultureSettings.NumericCulture),
                    crystal.DisplayBottom.ToString("F3", CultureSettings.NumericCulture)
                }));
            }
        }

        public void ExportToJson(string filePath, WaferInfo info, List<Crystal> crystals)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine("{");
            writer.WriteLine("  \"waferInfo\": {");
            writer.WriteLine($"    \"diameter\": \"{info.WaferDiameter.ToString(CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"crystalWidthUm\": \"{info.SizeX.ToString(CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"crystalHeightUm\": \"{info.SizeY.ToString(CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"stepXmm\": \"{info.StepXmm.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"stepYmm\": \"{info.StepYmm.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"streetMm\": \"{info.StreetMm.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"offsetXmm\": \"{info.OffsetXMm.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"offsetYmm\": \"{info.OffsetYMm.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"mirrorX\": \"{(info.MirrorX ? 1 : 0)}\",");
            writer.WriteLine($"    \"mirrorY\": \"{(info.MirrorY ? 1 : 0)}\",");
            writer.WriteLine($"    \"orientationSwapped\": \"{(info.OrientationSwapped ? 1 : 0)}\",");
            writer.WriteLine($"    \"edgeExclusionMm\": \"{info.EdgeExclusionMm.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"rotationDeg\": \"{info.RotationAngleDeg.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"zoom\": \"{info.ZoomFactor.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"panX\": \"{info.PanOffsetX.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"panY\": \"{info.PanOffsetY.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"pointerX\": \"{info.PointerXmm.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"pointerY\": \"{info.PointerYmm.ToString("F3", CultureSettings.NumericCulture)}\",");
            writer.WriteLine($"    \"calibrated\": \"{(info.HasCalibration ? 1 : 0)}\"");
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
                float angle = (float)(Math.Atan2(crystal.RealY, crystal.RealX) * 180.0 / Math.PI);
                string quadrant = DetermineQuadrant(crystal);

                writer.WriteLine("    {");
                writer.WriteLine($"      \"index\": \"{crystal.Index}\",");
                writer.WriteLine($"      \"x\": \"{crystal.RealX.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"y\": \"{crystal.RealY.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"widthMm\": \"{crystal.WidthMm.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"heightMm\": \"{crystal.HeightMm.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"status\": \"{crystal.PlacementStatus}\",");
                writer.WriteLine($"      \"distance\": \"{distance.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"angle\": \"{angle.ToString("F1", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"quadrant\": \"{quadrant}\",");
                writer.WriteLine($"      \"displayX\": \"{crystal.DisplayX.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"displayY\": \"{crystal.DisplayY.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"displayLeft\": \"{crystal.DisplayLeft.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"displayTop\": \"{crystal.DisplayTop.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"displayRight\": \"{crystal.DisplayRight.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"displayBottom\": \"{crystal.DisplayBottom.ToString("F3", CultureSettings.NumericCulture)}\",");
                writer.WriteLine($"      \"colorArgb\": \"{crystal.Color.ToArgb()}\"");
                writer.Write("    }");
                writer.WriteLine(i == crystals.Count - 1 ? string.Empty : ",");
            }

            writer.WriteLine("  ]");
            writer.WriteLine("}");
        }

        public (WaferInfo info, List<Crystal> crystals) ImportFromCompactXml(string filePath)
        {
            var info = new WaferInfo();
            var crystals = new List<Crystal>();

            using var reader = XmlReader.Create(filePath);
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (reader.Name)
                {
                    case "WaferInfo":
                        info.WaferDiameter = ParseUInt(reader.GetAttribute("diameter"), info.WaferDiameter);
                        info.SizeX = ParseUInt(reader.GetAttribute("crystalWidthUm"), info.SizeX);
                        info.SizeY = ParseUInt(reader.GetAttribute("crystalHeightUm"), info.SizeY);
                        info.StreetMm = ParseFloat(reader.GetAttribute("streetMm"), info.StreetMm);
                        info.OffsetXMm = ParseFloat(reader.GetAttribute("offsetXmm"), info.OffsetXMm);
                        info.OffsetYMm = ParseFloat(reader.GetAttribute("offsetYmm"), info.OffsetYMm);
                        info.MirrorX = ParseBool(reader.GetAttribute("mirrorX"));
                        info.MirrorY = ParseBool(reader.GetAttribute("mirrorY"));
                        info.OrientationSwapped = ParseBool(reader.GetAttribute("swap"));
                        info.EdgeExclusionMm = ParseFloat(reader.GetAttribute("edgeExclusionMm"), info.EdgeExclusionMm);
                        info.StepXmm = ParseFloat(reader.GetAttribute("stepXmm"), info.StepXmm);
                        info.StepYmm = ParseFloat(reader.GetAttribute("stepYmm"), info.StepYmm);
                        info.RotationAngleDeg = ParseFloat(reader.GetAttribute("rotationDeg"), info.RotationAngleDeg);
                        info.ZoomFactor = ParseFloat(reader.GetAttribute("zoom"), info.ZoomFactor);
                        info.PanOffsetX = ParseFloat(reader.GetAttribute("panX"), info.PanOffsetX);
                        info.PanOffsetY = ParseFloat(reader.GetAttribute("panY"), info.PanOffsetY);
                        info.PointerXmm = ParseFloat(reader.GetAttribute("pointerX"), info.PointerXmm);
                        info.PointerYmm = ParseFloat(reader.GetAttribute("pointerY"), info.PointerYmm);
                        info.HasCalibration = ParseBool(reader.GetAttribute("calibrated"));
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
                            Index = (int)ParseUInt(reader.GetAttribute("i")),
                            RealX = ParseFloat(reader.GetAttribute("x")),
                            RealY = ParseFloat(reader.GetAttribute("y")),
                            WidthMm = ParseFloat(reader.GetAttribute("w"), info.StepXmm > 0 ? info.StepXmm : Math.Max(0.01f, info.SizeX / 1000f)),
                            HeightMm = ParseFloat(reader.GetAttribute("h"), info.StepYmm > 0 ? info.StepYmm : Math.Max(0.01f, info.SizeY / 1000f))
                        };

                        string status = reader.GetAttribute("status");
                        if (!Enum.TryParse(status, true, out CrystalPlacementStatus placementStatus))
                        {
                            placementStatus = CrystalPlacementStatus.Full;
                        }
                        crystal.PlacementStatus = placementStatus;

                        string colorAttr = reader.GetAttribute("color");
                        if (!string.IsNullOrWhiteSpace(colorAttr) && int.TryParse(colorAttr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int argb))
                        {
                            crystal.Color = Color.FromArgb(argb);
                        }
                        else
                        {
                            crystal.Color = Color.Blue;
                        }

                        crystals.Add(crystal);
                        break;
                }
            }

            return (info, crystals);
        }

        public List<Crystal> ImportFromCsv(string filePath)
        {
            var crystals = new List<Crystal>();
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                var parts = line.Split(';');
                if (parts.Length < 3)
                {
                    continue;
                }

                var crystal = new Crystal
                {
                    Index = (int)ParseUInt(parts[0]),
                    RealX = ParseFloat(parts.ElementAtOrDefault(1)),
                    RealY = ParseFloat(parts.ElementAtOrDefault(2)),
                    WidthMm = ParseFloat(parts.ElementAtOrDefault(7)),
                    HeightMm = ParseFloat(parts.ElementAtOrDefault(8))
                };

                if (crystal.WidthMm <= 0f) crystal.WidthMm = 1f;
                if (crystal.HeightMm <= 0f) crystal.HeightMm = 1f;

                string status = parts.ElementAtOrDefault(9);
                if (!Enum.TryParse(status, true, out CrystalPlacementStatus placementStatus))
                {
                    placementStatus = CrystalPlacementStatus.Full;
                }
                crystal.PlacementStatus = placementStatus;

                string colorName = parts.ElementAtOrDefault(6);
                if (!string.IsNullOrWhiteSpace(colorName))
                {
                    try
                    {
                        crystal.Color = Color.FromName(colorName);
                    }
                    catch
                    {
                        crystal.Color = Color.Blue;
                    }
                }
                else
                {
                    crystal.Color = Color.Blue;
                }

                crystal.DisplayX = ParseFloat(parts.ElementAtOrDefault(10));
                crystal.DisplayY = ParseFloat(parts.ElementAtOrDefault(11));
                crystal.DisplayLeft = ParseFloat(parts.ElementAtOrDefault(12));
                crystal.DisplayTop = ParseFloat(parts.ElementAtOrDefault(13));
                crystal.DisplayRight = ParseFloat(parts.ElementAtOrDefault(14));
                crystal.DisplayBottom = ParseFloat(parts.ElementAtOrDefault(15));

                crystals.Add(crystal);
            }

            return crystals;
        }

        private static string DetermineQuadrant(Crystal crystal)
        {
            if (crystal.RealX >= 0 && crystal.RealY >= 0) return "Q1";
            if (crystal.RealX < 0 && crystal.RealY >= 0) return "Q2";
            if (crystal.RealX < 0 && crystal.RealY < 0) return "Q3";
            return "Q4";
        }

        private static float ParseFloat(string source, float fallback = 0f)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return fallback;
            }

            return float.TryParse(source, NumberStyles.Float, CultureSettings.NumericCulture, out float value)
                ? value
                : fallback;
        }

        private static uint ParseUInt(string source, uint fallback = 0)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return fallback;
            }

            return uint.TryParse(source, NumberStyles.Integer, CultureSettings.NumericCulture, out uint value)
                ? value
                : fallback;
        }

        private static bool ParseBool(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return source == "1" || source.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public ExportFormat GetFormatFromExtension(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".xml" => ExportFormat.Xml,
                ".csv" => ExportFormat.Csv,
                ".json" => ExportFormat.Json,
                _ => ExportFormat.Unknown
            };
        }
    }

    public enum ExportFormat
    {
        Unknown,
        Xml,
        Csv,
        Json
    }
}
