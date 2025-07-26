using WaferVision.Core.Domain;

namespace WaferVision.Core.Services.Interfaces;

public interface IGridCache
{
    IEnumerable<Crystal> GetOrCreate(GridParameters parameters);
}
