namespace CS2Toolkit.Models.Abstractions;

public sealed record BonePosition(int Index, Vector3 Position, bool IsValid);

public sealed record PlayerBones(IReadOnlyList<BonePosition> Bones)
{
    public const int Count = 28;
    public const int MatrixStride = 32;
    public const float MaxConnectionWorldDistance = 120f;

    public static readonly int[] RequiredIndices =
    [
        (int)BoneId.Pelvis, (int)BoneId.Neck, (int)BoneId.Head, (int)BoneId.Chest,
        (int)BoneId.ShoulderA, (int)BoneId.ElbowA, (int)BoneId.HandA,
        (int)BoneId.ShoulderB, (int)BoneId.ElbowB, (int)BoneId.HandB,
        (int)BoneId.HipA, (int)BoneId.KneeA, (int)BoneId.AnkleA,
        (int)BoneId.HipB, (int)BoneId.KneeB, (int)BoneId.AnkleB
    ];

    public static readonly (int From, int To)[] Connections =
    [
        ((int)BoneId.Neck, (int)BoneId.Head),
        ((int)BoneId.Neck, (int)BoneId.ShoulderA),
        ((int)BoneId.Neck, (int)BoneId.ShoulderB),
        ((int)BoneId.ElbowA, (int)BoneId.ShoulderA),
        ((int)BoneId.ElbowB, (int)BoneId.ShoulderB),
        ((int)BoneId.HandA, (int)BoneId.ElbowA),
        ((int)BoneId.HandB, (int)BoneId.ElbowB),
        ((int)BoneId.Neck, (int)BoneId.Chest),
        ((int)BoneId.Chest, (int)BoneId.Pelvis),
        ((int)BoneId.HipA, (int)BoneId.Pelvis),
        ((int)BoneId.KneeA, (int)BoneId.HipA),
        ((int)BoneId.AnkleA, (int)BoneId.KneeA),
        ((int)BoneId.HipB, (int)BoneId.Pelvis),
        ((int)BoneId.KneeB, (int)BoneId.HipB),
        ((int)BoneId.AnkleB, (int)BoneId.KneeB)
    ];

    public static PlayerBones Empty { get; } = new(Array.Empty<BonePosition>());

    public bool HasValidSkeleton =>
        TryGetBone((int)BoneId.Pelvis, out _)
        && TryGetBone((int)BoneId.Neck, out _)
        && TryGetBone((int)BoneId.Head, out _);

    public bool TryGetBone(int index, out Vector3 position)
    {
        foreach (var bone in Bones)
        {
            if (bone.Index != index)
                continue;

            if (bone.IsValid)
            {
                position = bone.Position;
                return true;
            }

            position = default;
            return false;
        }

        position = default;
        return false;
    }
}
