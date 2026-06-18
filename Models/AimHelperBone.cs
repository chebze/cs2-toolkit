namespace Cs2Toolkit.Models;

public enum AimHelperBone
{
    Head,
    Neck,
    Body
}

public static class AimHelperBoneParser
{
    public static AimHelperBone Parse(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "neck" => AimHelperBone.Neck,
            "body" or "chest" => AimHelperBone.Body,
            _ => AimHelperBone.Head
        };

    public static string ToLabel(AimHelperBone bone) => bone switch
    {
        AimHelperBone.Neck => "N",
        AimHelperBone.Body => "B",
        _ => "H"
    };
}
