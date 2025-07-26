using WaferVision.Core.Domain;
using WaferVision.Core.Services.Interfaces;

namespace WaferVision.Core.Services.Implementations;

public class GridCache : IGridCache
{
    private readonly Dictionary<int, IEnumerable<Crystal>> _cache = new();
    private readonly ICrystalGenerator _generator;

    public GridCache(ICrystalGenerator generator)
    {
        _generator = generator;
    }

    private static int ComputeHash(GridParameters p) =>
        HashCode.Combine(p.DieWidth, p.DieHeight, p.StreetWidth, p.OffsetX, p.OffsetY);

    public IEnumerable<Crystal> GetOrCreate(GridParameters parameters)
    {
        int hash = ComputeHash(parameters);
        if (!_cache.TryGetValue(hash, out var list))
        {
            var wafer = new Wafer { Diameter = 0, Grid = parameters };
            list = _generator.Generate(wafer);
            _cache[hash] = list.ToList();
        }
        return list;
    }
}
