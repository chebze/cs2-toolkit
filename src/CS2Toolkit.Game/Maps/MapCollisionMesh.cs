using System.Numerics;

namespace CS2Toolkit.Game.Maps;

public sealed class MapCollisionMesh
{
    public required string MapName { get; init; }
    public required Vector3[] Vertices { get; init; }
    public required int[] Indices { get; init; }
    public int TriangleCount => Indices.Length / 3;
}
