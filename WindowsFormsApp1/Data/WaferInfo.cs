using System;
using System.Globalization;
using System.Xml.Serialization;
using CrystalTable;

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

        [XmlElement]
        public bool HasCalibration { get; set; }

        [XmlIgnore]
        public float StepXmm { get; set; }

        [XmlIgnore]
        public float StepYmm { get; set; }

        [XmlIgnore]
        public float RotationAngleDeg { get; set; }

        [XmlIgnore]
        public float ZoomFactor { get; set; }

        [XmlIgnore]
        public float PanOffsetX { get; set; }

        [XmlIgnore]
        public float PanOffsetY { get; set; }

        [XmlIgnore]
        public float PointerXmm { get; set; }

        [XmlIgnore]
        public float PointerYmm { get; set; }

        [XmlIgnore]
        public float FirstReferenceX { get; set; }

        [XmlIgnore]
        public float FirstReferenceY { get; set; }

        [XmlIgnore]
        public float LastReferenceX { get; set; }

        [XmlIgnore]
        public float LastReferenceY { get; set; }

        [XmlIgnore]
        public float StreetMm { get; set; }

        [XmlIgnore]
        public float OffsetXMm { get; set; }

        [XmlIgnore]
        public float OffsetYMm { get; set; }

        [XmlIgnore]
        public bool OrientationSwapped { get; set; }

        [XmlIgnore]
        public bool MirrorX { get; set; }

        [XmlIgnore]
        public bool MirrorY { get; set; }

        [XmlIgnore]
        public float EdgeExclusionMm { get; set; }

        [XmlElement("StepXmm")]
        public string StepXmmSerialized
        {
            get => StepXmm.ToString("F3", CultureSettings.NumericCulture);
            set => StepXmm = ParseFloat(value);
        }

        [XmlElement("StepYmm")]
        public string StepYmmSerialized
        {
            get => StepYmm.ToString("F3", CultureSettings.NumericCulture);
            set => StepYmm = ParseFloat(value);
        }

        [XmlElement("RotationAngleDeg")]
        public string RotationSerialized
        {
            get => RotationAngleDeg.ToString("F3", CultureSettings.NumericCulture);
            set => RotationAngleDeg = ParseFloat(value);
        }

        [XmlElement("ZoomFactor")]
        public string ZoomSerialized
        {
            get => ZoomFactor.ToString("F3", CultureSettings.NumericCulture);
            set => ZoomFactor = ParseFloat(value, defaultValue: 1f);
        }

        [XmlElement("PanOffsetX")]
        public string PanOffsetXSerialized
        {
            get => PanOffsetX.ToString("F3", CultureSettings.NumericCulture);
            set => PanOffsetX = ParseFloat(value);
        }

        [XmlElement("PanOffsetY")]
        public string PanOffsetYSerialized
        {
            get => PanOffsetY.ToString("F3", CultureSettings.NumericCulture);
            set => PanOffsetY = ParseFloat(value);
        }

        [XmlElement("PointerXmm")]
        public string PointerXSerialized
        {
            get => PointerXmm.ToString("F3", CultureSettings.NumericCulture);
            set => PointerXmm = ParseFloat(value);
        }

        [XmlElement("PointerYmm")]
        public string PointerYSerialized
        {
            get => PointerYmm.ToString("F3", CultureSettings.NumericCulture);
            set => PointerYmm = ParseFloat(value);
        }

        [XmlElement("FirstRefX")]
        public string FirstReferenceXSerialized
        {
            get => FirstReferenceX.ToString("F3", CultureSettings.NumericCulture);
            set => FirstReferenceX = ParseFloat(value);
        }

        [XmlElement("FirstRefY")]
        public string FirstReferenceYSerialized
        {
            get => FirstReferenceY.ToString("F3", CultureSettings.NumericCulture);
            set => FirstReferenceY = ParseFloat(value);
        }

        [XmlElement("LastRefX")]
        public string LastReferenceXSerialized
        {
            get => LastReferenceX.ToString("F3", CultureSettings.NumericCulture);
            set => LastReferenceX = ParseFloat(value);
        }

        [XmlElement("LastRefY")]
        public string LastReferenceYSerialized
        {
            get => LastReferenceY.ToString("F3", CultureSettings.NumericCulture);
            set => LastReferenceY = ParseFloat(value);
        }

        [XmlElement("StreetMm")]
        public string StreetSerialized
        {
            get => StreetMm.ToString("F3", CultureSettings.NumericCulture);
            set => StreetMm = ParseFloat(value);
        }

        [XmlElement("OffsetXMm")]
        public string OffsetXSerialized
        {
            get => OffsetXMm.ToString("F3", CultureSettings.NumericCulture);
            set => OffsetXMm = ParseFloat(value);
        }

        [XmlElement("OffsetYMm")]
        public string OffsetYSerialized
        {
            get => OffsetYMm.ToString("F3", CultureSettings.NumericCulture);
            set => OffsetYMm = ParseFloat(value);
        }

        [XmlElement("OrientationSwapped")]
        public bool OrientationSerialized
        {
            get => OrientationSwapped;
            set => OrientationSwapped = value;
        }

        [XmlElement("MirrorX")]
        public bool MirrorXSerialized
        {
            get => MirrorX;
            set => MirrorX = value;
        }

        [XmlElement("MirrorY")]
        public bool MirrorYSerialized
        {
            get => MirrorY;
            set => MirrorY = value;
        }

        [XmlElement("EdgeExclusionMm")]
        public string EdgeExclusionSerialized
        {
            get => EdgeExclusionMm.ToString("F3", CultureSettings.NumericCulture);
            set => EdgeExclusionMm = ParseFloat(value);
        }

        private static float ParseFloat(string source, float defaultValue = 0f)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return defaultValue;
            }

            if (float.TryParse(source, NumberStyles.Float, CultureSettings.NumericCulture, out float value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}
