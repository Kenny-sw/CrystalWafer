namespace WaferVision.Core.Services.Interfaces;

public interface IPositionerDriver
{
    Task MoveAbsoluteAsync(double x, double y, CancellationToken ct);
    Task MoveRelativeAsync(double dx, double dy, CancellationToken ct);
    Task<(double x, double y)> GetPositionAsync(CancellationToken ct);
    Task ScanAsync(IEnumerable<WaferVision.Core.Domain.Crystal> points, CancellationToken ct);
}
