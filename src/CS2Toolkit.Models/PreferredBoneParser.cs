using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Models;

public enum PreferredAimBone
{
    Head,
    Neck,
    Body
}

public static class PreferredBoneParser
{
    public static PreferredAimBone Parse(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "neck" => PreferredAimBone.Neck,
            "body" or "chest" => PreferredAimBone.Body,
            _ => PreferredAimBone.Head
        };

    public static string ToLabel(PreferredAimBone bone) => bone switch
    {
        PreferredAimBone.Neck => "N",
        PreferredAimBone.Body => "B",
        _ => "H"
    };

    public static IEnumerable<BoneId> GetPreferenceOrder(PreferredAimBone preferred) => preferred switch
    {
        PreferredAimBone.Neck => [BoneId.Neck, BoneId.Head, BoneId.Chest],
        PreferredAimBone.Body => [BoneId.Chest, BoneId.Neck, BoneId.Head],
        _ => [BoneId.Head, BoneId.Neck, BoneId.Chest]
    };
}
