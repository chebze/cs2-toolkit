using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

internal sealed class AutoStopper
{
    private readonly HashSet<KeyCode> _counterKeysHeld = [];

    public void Reset(IInputSimulator input) => ReleaseCounterKeys(input);

    public bool TryEnsureStopped(
        IInputSimulator input,
        TriggerbotState triggerbot,
        TriggerbotHostSettings options)
    {
        var velocity = triggerbot.Velocity;
        var horizontalSpeed = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
        if (horizontalSpeed <= options.AutoStopSpeedThreshold)
        {
            ReleaseCounterKeys(input);
            return true;
        }

        ApplyCounterKeys(input, velocity);
        return false;
    }

    private void ApplyCounterKeys(IInputSimulator input, Vector3 velocity)
    {
        var desired = new HashSet<KeyCode>();
        if (velocity.X > 1f)
            desired.Add(VirtualKeys.D);
        else if (velocity.X < -1f)
            desired.Add(VirtualKeys.A);

        if (velocity.Y > 1f)
            desired.Add(VirtualKeys.S);
        else if (velocity.Y < -1f)
            desired.Add(VirtualKeys.W);

        foreach (var key in _counterKeysHeld.Where(key => !desired.Contains(key)).ToList())
        {
            input.SetKeyState(key, false);
            _counterKeysHeld.Remove(key);
        }

        foreach (var key in desired.Where(key => !_counterKeysHeld.Contains(key)))
        {
            input.SetKeyState(key, true);
            _counterKeysHeld.Add(key);
        }
    }

    private void ReleaseCounterKeys(IInputSimulator input)
    {
        foreach (var key in _counterKeysHeld)
            input.SetKeyState(key, false);

        _counterKeysHeld.Clear();
    }
}
