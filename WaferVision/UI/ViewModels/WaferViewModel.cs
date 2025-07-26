using System.Collections.ObjectModel;
using WaferVision.Core.Domain;
using WaferVision.Core.Services.Implementations;

namespace WaferVision.UI.ViewModels;

public class WaferViewModel
{
    public ObservableCollection<Crystal> Crystals { get; } = new();

    public WaferViewModel()
    {
        var grid = new GridParameters { DieWidth = 1, DieHeight = 1, StreetWidth = 0.1 };
        var wafer = new Wafer { Diameter = 100, Grid = grid };
        var generator = new CrystalGenerator();
        var list = generator.Generate(wafer).Take(10000);
        foreach (var c in list)
            Crystals.Add(c);
    }
}
