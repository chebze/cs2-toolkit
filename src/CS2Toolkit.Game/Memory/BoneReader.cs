using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Memory;

internal static class BoneReader
{
    public static PlayerBones? TryReadSkeleton(ProcessMemory memory, GameOffsets offsets, nint pawn)
    {
        if (pawn == nint.Zero)
            return null;

        var sceneNode = memory.ReadPtr(pawn + offsets.M_pGameSceneNode);
        if (sceneNode == nint.Zero)
            return null;

        var boneArray = memory.ReadPtr(sceneNode + offsets.M_modelState + GameOffsets.ModelStateBoneArray);
        if (boneArray == nint.Zero)
            return null;

        var bones = new List<BonePosition>(PlayerBones.RequiredIndices.Length);
        foreach (var boneId in PlayerBones.RequiredIndices)
        {
            var boneAddress = boneArray + (nint)(boneId * PlayerBones.MatrixStride);
            var position = new Vector3(
                memory.Read<float>(boneAddress),
                memory.Read<float>(boneAddress + 4),
                memory.Read<float>(boneAddress + 8));

            bones.Add(new BonePosition(boneId, position, position.IsValid));
        }

        var result = new PlayerBones(bones);
        return result.HasValidSkeleton ? result : null;
    }
}
