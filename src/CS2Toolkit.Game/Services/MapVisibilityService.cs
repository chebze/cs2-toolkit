using CS2Toolkit.Game.Abstractions;
using CS2Toolkit.Game.Maps;
using ModelVector3 = CS2Toolkit.Models.Abstractions.Vector3;
using NumericVector3 = System.Numerics.Vector3;

namespace CS2Toolkit.Game.Services;

public sealed class MapVisibilityService : IMapVisibility
{
    private readonly MapVisibilityChecker _checker;

    public MapVisibilityService(MapVisibilityChecker checker) => _checker = checker;

    public bool IsReady => _checker.IsReady;

    public int LoadedMapCount => _checker.LoadedMapCount;

    public void SetActiveMap(string? mapName) => _checker.SetActiveMap(mapName);

    public bool HasLineOfSight(ModelVector3 from, ModelVector3 to) =>
        _checker.TryHasLineOfSight(ToNumeric(from), ToNumeric(to));

    private static NumericVector3 ToNumeric(ModelVector3 vector) => new(vector.X, vector.Y, vector.Z);
}
