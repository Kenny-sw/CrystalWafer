using WaferVision.Core.Domain;
using WaferVision.Core.Services.Interfaces;

namespace WaferVision.Core.Services.Implementations;

public class PositionerEmulator : IPositionerDriver
{
    private double _x;
    private double _y;

    public Task MoveAbsoluteAsync(double x, double y, CancellationToken ct)
    {
        _x = x;
        _y = y;
        return Task.CompletedTask;
    }

    public Task MoveRelativeAsync(double dx, double dy, CancellationToken ct)
    {
        _x += dx;
        _y += dy;
        return Task.CompletedTask;
    }

    public Task<(double x, double y)> GetPositionAsync(CancellationToken ct)
    {
        return Task.FromResult((_x, _y));
    }

    public async Task ScanAsync(IEnumerable<Crystal> points, CancellationToken ct)
    {
        foreach (var p in points)
        {
            await MoveAbsoluteAsync(p.CenterX, p.CenterY, ct);
            await Task.Delay(10, ct);
        }
    }
}
