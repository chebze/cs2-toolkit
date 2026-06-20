namespace Cs2Toolkit.Models;

public sealed class EnemyLastSeenSnapshot
{
    public int PlayerIndex { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Health { get; init; }
    public Vector3[] Bones { get; init; } = new Vector3[PlayerBones.Count];
    public DateTime LastSeenAt { get; init; }

    public bool HasValidBones =>
        Bones[PlayerBones.Pelvis].IsValid
        && Bones[PlayerBones.Neck].IsValid
        && Bones[PlayerBones.Head].IsValid;
}
