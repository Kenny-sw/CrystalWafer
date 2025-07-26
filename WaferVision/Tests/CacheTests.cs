using WaferVision.Core.Domain;
using WaferVision.Core.Services.Implementations;
using Xunit;

namespace WaferVision.Tests;

public class CacheTests
{
    [Fact]
    public void Cache_ReturnsSameCollection_ForSameParameters()
    {
        var p = new GridParameters { DieWidth = 1, DieHeight = 1, StreetWidth = 0.1 };
        var cache = new GridCache(new CrystalGenerator());
        var first = cache.GetOrCreate(p);
        var second = cache.GetOrCreate(p);
        Assert.Same(first, second);
    }
}
