using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class TriggerbotController
{
    private enum TriggerPhase
    {
        Idle,
        PreFire,
        OnTarget,
        PostFire
    }

    private readonly AutoStopper _autoStopper = new();
    private TriggerPhase _phase = TriggerPhase.Idle;
    private int _preFireBudget;
    private int _postFireBudget;
    private int _graceShotsBaseline;
    private bool _syntheticMouseDown;
    private int _reactionDelayMs;
    private DateTimeOffset _reactionAcquiredAt;

    public void Reset(IInputSimulator input)
    {
        ReleaseSyntheticFire(input);
        _autoStopper.Reset(input);
        _phase = TriggerPhase.Idle;
        _preFireBudget = 0;
        _postFireBudget = 0;
        _graceShotsBaseline = 0;
        _reactionDelayMs = 0;
    }

    public void Process(
        FeatureContext context,
        bool autoStopEnabled)
    {
        var input = context.Input;
        var snapshot = context.Snapshot;
        var triggerbot = snapshot.Triggerbot;
        var host = context.Settings.Host.Triggerbot;
        var weapon = context.WeaponSettings.Triggerbot;

        if (!snapshot.IsAttached || !snapshot.IsInMatch || snapshot.LocalPlayer is null)
        {
            Reset(input);
            return;
        }

        if (input.IsKeyDown(VirtualKeys.LeftMouse))
        {
            Reset(input);
            return;
        }

        if (triggerbot.IsReloading)
        {
            ReleaseSyntheticFire(input);
            return;
        }

        var preFireFovDegrees = weapon.PreFireFovDegrees ?? 0.7f;
        var minReactionDelayMs = weapon.MinReactionDelayMs ?? 200;
        var maxReactionDelayMs = weapon.MaxReactionDelayMs ?? 400;
        var onTarget = triggerbot.CrosshairOnEnemy;
        var nearTarget = !onTarget && triggerbot.IsNearVisibleEnemy(preFireFovDegrees);
        var hasAcquisition = onTarget || nearTarget;

        if (!hasAcquisition && _phase != TriggerPhase.PostFire)
            ClearReactionDelay();
        else if (_phase == TriggerPhase.Idle && hasAcquisition)
            BeginReactionDelayIfNeeded(minReactionDelayMs, maxReactionDelayMs);

        var delayElapsed = IsReactionDelayElapsed();
        var shotsFired = triggerbot.ShotsFired;

        if (onTarget)
        {
            if (!delayElapsed)
            {
                ReleaseSyntheticFire(input);
                return;
            }

            if (_phase is TriggerPhase.Idle or TriggerPhase.PreFire)
                _postFireBudget = RollGraceBulletBudget(host);

            _phase = TriggerPhase.OnTarget;
            if (!TryBeginFiring(input, triggerbot, autoStopEnabled, host))
                return;

            return;
        }

        if (_phase == TriggerPhase.OnTarget)
        {
            _phase = TriggerPhase.PostFire;
            _graceShotsBaseline = shotsFired;
        }

        if (_phase == TriggerPhase.PostFire)
        {
            if (shotsFired < _graceShotsBaseline + _postFireBudget)
            {
                if (!TryBeginFiring(input, triggerbot, autoStopEnabled, host))
                    return;

                return;
            }

            ReleaseSyntheticFire(input);
            _phase = TriggerPhase.Idle;
            return;
        }

        if (nearTarget)
        {
            if (_phase == TriggerPhase.Idle)
            {
                _preFireBudget = RollGraceBulletBudget(host);
                _graceShotsBaseline = shotsFired;
                _phase = TriggerPhase.PreFire;
            }

            if (_phase == TriggerPhase.PreFire)
            {
                if (!delayElapsed)
                {
                    ReleaseSyntheticFire(input);
                    return;
                }

                if (shotsFired < _graceShotsBaseline + _preFireBudget)
                {
                    if (!TryBeginFiring(input, triggerbot, autoStopEnabled, host))
                        return;
                }
                else
                {
                    ReleaseSyntheticFire(input);
                }

                return;
            }
        }
        else if (_phase == TriggerPhase.PreFire)
        {
            ReleaseSyntheticFire(input);
            _phase = TriggerPhase.Idle;
            return;
        }

        ReleaseSyntheticFire(input);
        _phase = TriggerPhase.Idle;
    }

    private bool TryBeginFiring(
        IInputSimulator input,
        TriggerbotState triggerbot,
        bool autoStopEnabled,
        TriggerbotHostSettings host)
    {
        if (autoStopEnabled && !_autoStopper.TryEnsureStopped(input, triggerbot, host))
        {
            ReleaseSyntheticFire(input);
            return false;
        }

        HoldSyntheticFire(input);
        return true;
    }

    private static int RollGraceBulletBudget(TriggerbotHostSettings host)
    {
        var min = Math.Min(host.MinGraceBullets, host.MaxGraceBullets);
        var max = Math.Max(host.MinGraceBullets, host.MaxGraceBullets);
        return Random.Shared.Next(min, max + 1);
    }

    private void BeginReactionDelayIfNeeded(int minReactionDelayMs, int maxReactionDelayMs)
    {
        if (_reactionDelayMs > 0)
            return;

        var min = Math.Min(minReactionDelayMs, maxReactionDelayMs);
        var max = Math.Max(minReactionDelayMs, maxReactionDelayMs);
        _reactionDelayMs = Random.Shared.Next(min, max + 1);
        _reactionAcquiredAt = DateTimeOffset.UtcNow;
    }

    private bool IsReactionDelayElapsed() =>
        _reactionDelayMs <= 0
        || (DateTimeOffset.UtcNow - _reactionAcquiredAt).TotalMilliseconds >= _reactionDelayMs;

    private void ClearReactionDelay() => _reactionDelayMs = 0;

    private void HoldSyntheticFire(IInputSimulator input)
    {
        if (_syntheticMouseDown)
            return;

        input.SetLeftButton(true);
        _syntheticMouseDown = true;
    }

    private void ReleaseSyntheticFire(IInputSimulator input)
    {
        if (!_syntheticMouseDown)
            return;

        input.SetLeftButton(false);
        _syntheticMouseDown = false;
    }
}
