using Cs2Toolkit.Models;

namespace Cs2Toolkit.Memory;

public readonly struct BoneReadContext
{
    public nint Pawn { get; init; }
    public nint SceneNode { get; init; }
    public nint BoneArray { get; init; }
    public Vector3 EntityOrigin { get; init; }
}
