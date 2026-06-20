using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Models;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class AimHelperController
{
    private readonly IWorldProjector _projector;
    private readonly IOverlayViewport _viewport;
    private readonly IKeybindMatcher _keybindMatcher;

    public AimHelperController(
        IWorldProjector projector,
        IOverlayViewport viewport,
        IKeybindMatcher keybindMatcher)
    {
        _projector = projector;
        _viewport = viewport;
        _keybindMatcher = keybindMatcher;
    }

    public void Process(FeatureContext context)
    {
        var snapshot = context.Snapshot;
        if (!snapshot.IsAttached || !snapshot.IsInMatch || snapshot.LocalPlayer is null)
            return;

        if (!IsActivationSatisfied(context))
            return;

        var settings = context.WeaponSettings.AimHelper;
        var fovDegrees = settings.FovDegrees ?? 3f;
        var preferredBone = PreferredBoneParser.Parse(settings.PreferredBone);

        if (!TrySelectTarget(snapshot.AimHelper.Candidates, preferredBone, fovDegrees, out var target))
            return;

        var screenWidth = _viewport.Width;
        var screenHeight = _viewport.Height;
        if (screenWidth <= 0 || screenHeight <= 0)
            return;

        if (!_projector.TryProject(
                target.BonePosition,
                snapshot.ViewMatrix,
                screenWidth,
                screenHeight,
                out var screenX,
                out var screenY))
        {
            return;
        }

        var deltaX = (int)MathF.Round(screenX - screenWidth * 0.5f);
        var deltaY = (int)MathF.Round(screenY - screenHeight * 0.5f);
        if (deltaX != 0 || deltaY != 0)
            context.Input.MoveMouseRelative(deltaX, deltaY);
    }

    private bool IsActivationSatisfied(FeatureContext context)
    {
        var activationKey = context.Settings.Keybinds.AimHelperActivationKey;
        if (string.IsNullOrWhiteSpace(activationKey))
            return true;

        var key = _keybindMatcher.ParseKey(activationKey);
        return !key.IsNone && context.Input.IsKeyDown(key);
    }

    private static bool TrySelectTarget(
        IReadOnlyList<AimCandidate> candidates,
        PreferredAimBone preferredBone,
        float fovDegrees,
        out AimCandidate target)
    {
        target = default!;

        var bestAngle = float.MaxValue;
        var found = false;

        foreach (var bone in PreferredBoneParser.GetPreferenceOrder(preferredBone))
        {
            foreach (var candidate in candidates)
            {
                if (candidate.Bone != bone || candidate.AngularDistanceDegrees > fovDegrees)
                    continue;

                if (candidate.AngularDistanceDegrees >= bestAngle)
                    continue;

                bestAngle = candidate.AngularDistanceDegrees;
                target = candidate;
                found = true;
            }

            if (found)
                return true;
        }

        return false;
    }
}
