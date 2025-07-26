using WaferVision.Core.Domain;
using WaferVision.Core.Services.Implementations;
using Xunit;

namespace WaferVision.Tests;

public class GridTests
{
    [Fact]
    public void ConvertCoordinates_Correct()
    {
        var gp = new GridParameters { DieWidth = 1, DieHeight = 1, StreetWidth = 0.1 };
        var wafer = new Wafer { Diameter = 100, Grid = gp };
        var gen = new CrystalGenerator();
        var crystal = gen.Generate(wafer).First();
        Assert.Equal(0, crystal.CenterX, 6);
        Assert.Equal(0, crystal.CenterY, 6);
    }
}
