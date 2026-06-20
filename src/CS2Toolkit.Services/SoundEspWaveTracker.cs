using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

public sealed class SoundEspWaveTracker
{
    private readonly object _lock = new();
    private readonly List<ActiveNoiseWave> _waves = [];
    private Vector3? _bombPosition;
    private DateTimeOffset _bombWaveEpoch = DateTimeOffset.UtcNow;
    private bool _bombWasActive;

    public SoundEspWaveState CopyState()
    {
        lock (_lock)
        {
            return new SoundEspWaveState(
                _waves.ToList(),
                _bombPosition,
                _bombWaveEpoch);
        }
    }

    public void Update(GameSnapshot snapshot, SoundEspProfileOptions options)
    {
        if (!options.Enabled)
        {
            Reset();
            return;
        }

        if (!snapshot.IsAttached || !snapshot.IsInMatch)
        {
            Reset();
            return;
        }

        var now = DateTimeOffset.UtcNow;

        lock (_lock)
        {
            for (var i = _waves.Count - 1; i >= 0; i--)
            {
                if ((now - _waves[i].StartedAt).TotalMilliseconds >= options.WaveDurationMs)
                    _waves.RemoveAt(i);
            }

            foreach (var sound in snapshot.RecentSounds)
            {
                if (!IsWithinDistance(snapshot, sound.Position, options.MaxDistanceUnits))
                    continue;

                _waves.Add(new ActiveNoiseWave(sound.Position, sound.Timestamp));
            }

            UpdateBombState(snapshot.Bomb, now);
        }
    }

    private void UpdateBombState(BombState bomb, DateTimeOffset now)
    {
        var hasBombWaves = bomb.WorldPosition is { IsValid: true }
            && bomb.Status is BombStatus.Planted or BombStatus.Defusing;

        if (!hasBombWaves)
        {
            _bombPosition = null;
            _bombWasActive = false;
            return;
        }

        if (!_bombWasActive)
            _bombWaveEpoch = now;

        _bombPosition = bomb.WorldPosition;
        _bombWasActive = true;
    }

    private static bool IsWithinDistance(GameSnapshot snapshot, Vector3 position, float maxDistanceUnits)
    {
        var localPlayer = snapshot.Players.FirstOrDefault(player => player.IsLocalPlayer);
        if (localPlayer?.WorldPosition is not { } localPosition)
            return true;

        return localPosition.DistanceTo(position) <= maxDistanceUnits;
    }

    private void Reset()
    {
        lock (_lock)
        {
            _waves.Clear();
            _bombPosition = null;
            _bombWasActive = false;
        }
    }
}

public sealed record SoundEspWaveState(
    IReadOnlyList<ActiveNoiseWave> Waves,
    Vector3? BombPosition,
    DateTimeOffset BombWaveEpoch);

public sealed record ActiveNoiseWave(Vector3 WorldPosition, DateTimeOffset StartedAt);
