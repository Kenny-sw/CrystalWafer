using WaferVision.Core.Domain;

namespace WaferVision.Core.Services.Interfaces;

public interface ICrystalGenerator
{
    IEnumerable<Crystal> Generate(Wafer wafer);
}
