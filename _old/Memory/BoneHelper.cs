using Cs2Toolkit.Models;

namespace Cs2Toolkit.Memory;

internal static class BoneHelper
{
    public static bool TryReadSkeleton(
        ProcessMemory memory,
        GameOffsets offsets,
        nint pawn,
        Span<Vector3> bones)
    {
        bones.Clear();

        var sceneNode = memory.ReadPtr(pawn + offsets.M_pGameSceneNode);
        if (sceneNode == nint.Zero)
            return false;

        var boneArray = memory.ReadPtr(sceneNode + offsets.M_modelState + GameOffsets.ModelStateBoneArray);
        if (boneArray == nint.Zero)
            return false;

        foreach (var boneId in PlayerBones.RequiredIndices)
        {
            var boneAddress = boneArray + (nint)(boneId * PlayerBones.MatrixStride);
            bones[boneId] = new Vector3(
                memory.Read<float>(boneAddress),
                memory.Read<float>(boneAddress + 4),
                memory.Read<float>(boneAddress + 8));
        }

        return bones[PlayerBones.Pelvis].IsValid
            && bones[PlayerBones.Neck].IsValid
            && bones[PlayerBones.Head].IsValid;
    }
}
