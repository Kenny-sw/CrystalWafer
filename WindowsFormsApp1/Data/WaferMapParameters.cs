namespace CrystalTable.Data
{
    public class WaferMapParameters
    {
        public float DiameterMm { get; set; }
        public float CrystalWidthMm { get; set; }
        public float CrystalHeightMm { get; set; }
        public float StreetMm { get; set; }
        public float OffsetXMm { get; set; }
        public float OffsetYMm { get; set; }
        public bool SwapOrientation { get; set; }
        public bool MirrorX { get; set; }
        public bool MirrorY { get; set; }
        public float EdgeExclusionMm { get; set; }
        public float NotchAngleDeg { get; set; }
        public float NotchWidthMm { get; set; }

        public WaferMapParameters Clone()
        {
            return (WaferMapParameters)MemberwiseClone();
        }
    }
}
