using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Abstractions;

public interface IMapCatalog
{
    string? CurrentMap { get; }
}

public interface IMapVisibility
{
    bool HasLineOfSight(Vector3 from, Vector3 to);
}
