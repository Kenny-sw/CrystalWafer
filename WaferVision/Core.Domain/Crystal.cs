namespace WaferVision.Core.Domain;

public class Crystal
{
    public int Row { get; init; }
    public int Col { get; init; }
    public double CenterX { get; init; }
    public double CenterY { get; init; }
    public double? Value { get; set; }
}
