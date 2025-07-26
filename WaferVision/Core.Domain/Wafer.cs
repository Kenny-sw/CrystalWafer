namespace WaferVision.Core.Domain;

public class Wafer
{
    public int Diameter { get; init; }
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public GridParameters Grid { get; set; } = null!;
}
