using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Tests;

internal static class GameSnapshotTestSupport
{
    public static PlayerBones ValidSkeleton { get; } = new(
    [
        new BonePosition((int)BoneId.Pelvis, new Vector3(0f, 0f, 0f), true),
        new BonePosition((int)BoneId.Neck, new Vector3(0f, 0f, 10f), true),
        new BonePosition((int)BoneId.Head, new Vector3(0f, 0f, 20f), true)
    ]);

    public static LocalPlayer CreateLocalPlayer(Team team = Team.CounterTerrorist) =>
        new(
            new PlayerId(1),
            team,
            100,
            new WeaponId(7),
            "AK-47",
            WeaponType.Rifle,
            new Vector3(0f, 0f, 0f),
            default);

    public static Player LocalPlayerEntity(LocalPlayer local) =>
        new(
            local.Id,
            "Local",
            local.Team,
            local.Health,
            true,
            true,
            local.Position,
            ValidSkeleton);

    public static Player Enemy(
        int id,
        Team team = Team.Terrorist,
        bool spottedByTeam = true,
        bool visibleToLocal = false,
        bool alive = true) =>
        new(
            new PlayerId(id),
            $"Enemy{id}",
            team,
            alive ? 100 : 0,
            alive,
            false,
            new Vector3(100f + id, 0f, 0f),
            ValidSkeleton,
            spottedByTeam,
            visibleToLocal);

    public static GameSnapshot CreateInMatch(
        IReadOnlyList<Player> players,
        LocalPlayer? localPlayer = null,
        TriggerbotState? triggerbot = null,
        RcsState? rcs = null,
        AimHelperState? aimHelper = null,
        RoundState? round = null,
        IReadOnlyList<BulletImpactEvent>? bulletImpacts = null) =>
        new(
            DateTimeOffset.UtcNow,
            IsAttached: true,
            IsInGame: true,
            IsInMatch: true,
            MapName: "de_dust2",
            LocalPlayer: localPlayer ?? CreateLocalPlayer(),
            Players: players,
            Round: round ?? RoundState.Empty,
            Bomb: BombState.Hidden,
            BombSites: BombSitesInfo.Empty,
            ViewMatrix: new ViewMatrix(stackalloc float[ViewMatrix.FloatCount]),
            RecentSounds: [],
            RecentBulletImpacts: bulletImpacts ?? [],
            Grenades: [],
            ClairvoyanceTips: [],
            EnemiesAlive: 0,
            EnemiesDead: 0,
            TeammatesAlive: 0,
            TeammatesDead: 0,
            Triggerbot: triggerbot ?? TriggerbotState.Inactive,
            Rcs: rcs ?? RcsState.Inactive,
            AimHelper: aimHelper ?? AimHelperState.Inactive,
            Radar: RadarSnapshot.Idle);

    public static FeatureContext CreateContext(
        GameSnapshot snapshot,
        RecordingInputSimulator input,
        ResolvedWeaponSettings? weaponSettings = null,
        GlobalKeybinds? keybinds = null) =>
        new()
        {
            Snapshot = snapshot,
            Input = input,
            Settings = new ToolkitSettings
            {
                Host = new ToolkitHostSettings
                {
                    Triggerbot = new TriggerbotHostSettings
                    {
                        MinGraceBullets = 1,
                        MaxGraceBullets = 1,
                        AutoStopSpeedThreshold = 1000f
                    }
                },
                Keybinds = keybinds ?? new GlobalKeybinds()
            },
            WeaponSettings = weaponSettings ?? new ResolvedWeaponSettings
            {
                Triggerbot = new TriggerbotLayerSettings
                {
                    PreFireFovDegrees = 0.7f,
                    MinReactionDelayMs = 0,
                    MaxReactionDelayMs = 0
                },
                Rcs = new RcsLayerSettings
                {
                    Sensitivity = 1f,
                    PitchScale = 1f,
                    YawScale = 1f,
                    FirstBulletCompensateChance = 1f,
                    SubsequentBulletSkipChance = 0f
                },
                AimHelper = new AimHelperLayerSettings
                {
                    FovDegrees = 5f,
                    PreferredBone = "head"
                }
            }
        };
}

internal sealed class RecordingInputSimulator : IInputSimulator
{
    private readonly HashSet<int> _keysDown = [];
    private readonly List<(int X, int Y)> _mouseMoves = [];
    private bool _leftButtonDown;

    public IReadOnlyList<(int X, int Y)> MouseMoves => _mouseMoves;
    public bool LeftButtonDown => _leftButtonDown;

    public void SetKeyDown(int virtualKey, bool down)
    {
        if (down)
            _keysDown.Add(virtualKey);
        else
            _keysDown.Remove(virtualKey);
    }

    public bool IsKeyDown(KeyCode key) => _keysDown.Contains(key.VirtualKey);

    public (int X, int Y) GetCursorPosition() => (960, 540);

    public MouseButton GetPressedMouseButtons() =>
        _leftButtonDown ? MouseButton.Left : MouseButton.None;

    public void MoveMouseRelative(int deltaX, int deltaY) =>
        _mouseMoves.Add((deltaX, deltaY));

    public void SetLeftButton(bool pressed) => _leftButtonDown = pressed;

    public void SetKeyState(KeyCode key, bool pressed)
    {
        if (pressed)
            _keysDown.Add(key.VirtualKey);
        else
            _keysDown.Remove(key.VirtualKey);
    }
}

internal sealed class FixedProjector(float x, float y) : IWorldProjector
{
    public bool TryProject(
        Vector3 world,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        out float screenX,
        out float screenY)
    {
        screenX = x;
        screenY = y;
        return true;
    }
}

internal sealed class FixedViewport(int width, int height) : IOverlayViewport
{
    public int Width => width;
    public int Height => height;
}
