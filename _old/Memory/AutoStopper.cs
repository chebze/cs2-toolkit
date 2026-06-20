using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;
using System.Windows.Forms;

namespace Cs2Toolkit.Memory;

public sealed class AutoStopper
{
    private readonly HashSet<Keys> _counterKeysHeld = [];

    public void Reset() => ReleaseCounterKeys();

    public bool TryEnsureStopped(ProcessMemory memory, nint localPawn, GameOffsets offsets, TbOptions options)
    {
        if (offsets.M_vecAbsVelocity == nint.Zero || localPawn == nint.Zero)
            return true;

        var velocity = ReadVector(memory, localPawn + offsets.M_vecAbsVelocity);
        var horizontalSpeed = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
        if (horizontalSpeed <= options.AutoStopSpeedThreshold)
        {
            ReleaseCounterKeys();
            return true;
        }

        ApplyCounterKeys(velocity);
        return false;
    }

    private void ApplyCounterKeys(Vector3 velocity)
    {
        var desired = new HashSet<Keys>();
        if (velocity.X > 1f)
            desired.Add(Keys.D);
        else if (velocity.X < -1f)
            desired.Add(Keys.A);

        if (velocity.Y > 1f)
            desired.Add(Keys.S);
        else if (velocity.Y < -1f)
            desired.Add(Keys.W);

        foreach (var key in _counterKeysHeld.Where(key => !desired.Contains(key)).ToList())
        {
            NativeInput.SetKeyState(key, false);
            _counterKeysHeld.Remove(key);
        }

        foreach (var key in desired.Where(key => !_counterKeysHeld.Contains(key)))
        {
            NativeInput.SetKeyState(key, true);
            _counterKeysHeld.Add(key);
        }
    }

    private void ReleaseCounterKeys()
    {
        foreach (var key in _counterKeysHeld)
            NativeInput.SetKeyState(key, false);

        _counterKeysHeld.Clear();
    }

    private static Vector3 ReadVector(ProcessMemory memory, nint address) =>
        new(
            memory.Read<float>(address),
            memory.Read<float>(address + 4),
            memory.Read<float>(address + 8));
}
