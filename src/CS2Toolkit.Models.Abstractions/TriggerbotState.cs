namespace CS2Toolkit.Models.Abstractions;

public sealed record TriggerbotState(
    bool CrosshairOnEnemy,
    bool IsReloading,
    int ShotsFired,
    Vector3 Velocity,
    Vector3 EyePosition,
    float NearestVisibleEnemyAngleDegrees,
    PlayerId? NearestVisibleEnemyId)
{
    public static TriggerbotState Inactive { get; } = new(
        false,
        false,
        0,
        default,
        default,
        float.MaxValue,
        null);

    public bool IsNearVisibleEnemy(float preFireFovDegrees) =>
        NearestVisibleEnemyAngleDegrees <= preFireFovDegrees;
}
