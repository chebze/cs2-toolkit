using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class RcsController
{
    private const float MouseYawPerDegree = 0.022f;

    private Vector3 _oldPunch;
    private int _lastShotsFired;
    private bool _compensateCurrentBullet = true;

    public void Reset() => ResetState();

    public void Process(FeatureContext context)
    {
        var input = context.Input;
        var rcs = context.Snapshot.Rcs;
        var settings = context.WeaponSettings.Rcs;

        if (!context.Snapshot.IsAttached || !context.Snapshot.IsInMatch)
        {
            ResetState();
            return;
        }

        if (!input.IsKeyDown(VirtualKeys.LeftMouse))
        {
            ResetState();
            return;
        }

        if (rcs.IsScoped || rcs.ShotsFired <= 1)
        {
            ResetState();
            return;
        }

        if (!rcs.HasAimPunch)
            return;

        var sensitivity = settings.Sensitivity ?? 1.25f;
        var pitchScale = settings.PitchScale ?? 2f;
        var yawScale = settings.YawScale ?? 2f;
        var firstBulletChance = settings.FirstBulletCompensateChance ?? 0.5f;
        var skipChance = settings.SubsequentBulletSkipChance ?? 0.2f;

        UpdateBulletCompensationDecision(rcs.ShotsFired, firstBulletChance, skipChance);

        var deltaPitch = (rcs.AimPunch.X - _oldPunch.X) * -1f;
        var deltaYaw = (rcs.AimPunch.Y - _oldPunch.Y) * -1f;

        if (_compensateCurrentBullet)
        {
            var mouseX = (int)(deltaYaw * yawScale / sensitivity / -MouseYawPerDegree);
            var mouseY = (int)(deltaPitch * pitchScale / sensitivity / MouseYawPerDegree);

            if (mouseX != 0 || mouseY != 0)
                input.MoveMouseRelative(mouseX, mouseY);
        }

        _oldPunch = rcs.AimPunch;
    }

    private void UpdateBulletCompensationDecision(
        int shotsFired,
        float firstBulletChance,
        float skipChance)
    {
        if (shotsFired == _lastShotsFired)
            return;

        if (shotsFired > _lastShotsFired)
        {
            _compensateCurrentBullet = shotsFired == 2
                ? Random.Shared.NextDouble() < firstBulletChance
                : Random.Shared.NextDouble() >= skipChance;
        }

        _lastShotsFired = shotsFired;
    }

    private void ResetState()
    {
        _oldPunch = default;
        _lastShotsFired = 0;
        _compensateCurrentBullet = true;
    }
}
