using WaferVision.Core.Domain;
using WaferVision.Core.Services.Interfaces;

namespace WaferVision.Core.Services.Implementations;

public class CrystalGenerator : ICrystalGenerator
{
    public IEnumerable<Crystal> Generate(Wafer wafer)
    {
        var grid = wafer.Grid;
        var crystals = new List<Crystal>();
        int rows = (int)(wafer.Diameter / grid.DieHeight);
        int cols = (int)(wafer.Diameter / grid.DieWidth);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int colIndex = r % 2 == 0 ? c : cols - c - 1;
                double x = grid.OffsetX + colIndex * (grid.DieWidth + grid.StreetWidth);
                double y = grid.OffsetY + r * (grid.DieHeight + grid.StreetWidth);

                crystals.Add(new Crystal
                {
                    Row = r,
                    Col = colIndex,
                    CenterX = x,
                    CenterY = y
                });
            }
        }
        return crystals;
    }
}
