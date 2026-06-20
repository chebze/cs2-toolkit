using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services;
using Moq;

namespace CS2Toolkit.Services.Tests;

public sealed class TriggerbotControllerTests
{
    [Fact]
    public void Process_when_detached_releases_synthetic_fire()
    {
        var controller = new TriggerbotController();
        var input = new RecordingInputSimulator();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var firingSnapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            triggerbot: new TriggerbotState(true, false, 0, default, default, float.MaxValue, null));

        controller.Process(GameSnapshotTestSupport.CreateContext(firingSnapshot, input), autoStopEnabled: false);
        Assert.True(input.LeftButtonDown);

        controller.Process(
            GameSnapshotTestSupport.CreateContext(GameSnapshot.Detached, input),
            autoStopEnabled: false);

        Assert.False(input.LeftButtonDown);
    }

    [Fact]
    public void Process_when_user_holds_left_mouse_does_not_synthesize_click()
    {
        var controller = new TriggerbotController();
        var input = new RecordingInputSimulator();
        input.SetKeyDown(0x01, true);

        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var snapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            triggerbot: new TriggerbotState(true, false, 0, default, default, float.MaxValue, null));

        controller.Process(GameSnapshotTestSupport.CreateContext(snapshot, input), autoStopEnabled: false);

        Assert.False(input.LeftButtonDown);
    }

    [Fact]
    public void Process_when_crosshair_on_enemy_presses_left_button()
    {
        var controller = new TriggerbotController();
        var input = new RecordingInputSimulator();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var snapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            triggerbot: new TriggerbotState(true, false, 0, default, default, float.MaxValue, null));

        controller.Process(GameSnapshotTestSupport.CreateContext(snapshot, input), autoStopEnabled: false);

        Assert.True(input.LeftButtonDown);
    }

    [Fact]
    public void Process_when_reloading_releases_synthetic_fire()
    {
        var controller = new TriggerbotController();
        var input = new RecordingInputSimulator();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var firingSnapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            triggerbot: new TriggerbotState(true, false, 0, default, default, float.MaxValue, null));

        controller.Process(GameSnapshotTestSupport.CreateContext(firingSnapshot, input), autoStopEnabled: false);
        Assert.True(input.LeftButtonDown);

        var reloadingSnapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            triggerbot: new TriggerbotState(true, true, 0, default, default, float.MaxValue, null));

        controller.Process(GameSnapshotTestSupport.CreateContext(reloadingSnapshot, input), autoStopEnabled: false);

        Assert.False(input.LeftButtonDown);
    }
}

public sealed class RcsControllerTests
{
    [Fact]
    public void Process_when_not_shooting_does_not_move_mouse()
    {
        var controller = new RcsController();
        var input = new RecordingInputSimulator();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var snapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            rcs: new RcsState(new Vector3(1f, 1f, 0f), 2, false, true));

        controller.Process(GameSnapshotTestSupport.CreateContext(snapshot, input));

        Assert.Empty(input.MouseMoves);
    }

    [Fact]
    public void Process_compensates_recoil_while_shooting()
    {
        var controller = new RcsController();
        var input = new RecordingInputSimulator();
        input.SetKeyDown(0x01, true);

        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var snapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            rcs: new RcsState(new Vector3(2f, -1f, 0f), 2, false, true));

        controller.Process(GameSnapshotTestSupport.CreateContext(snapshot, input));

        Assert.NotEmpty(input.MouseMoves);
    }

    [Fact]
    public void Process_when_detached_resets_state()
    {
        var controller = new RcsController();
        var input = new RecordingInputSimulator();
        input.SetKeyDown(0x01, true);

        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var inMatch = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            rcs: new RcsState(new Vector3(2f, -1f, 0f), 2, false, true));

        controller.Process(GameSnapshotTestSupport.CreateContext(inMatch, input));
        Assert.NotEmpty(input.MouseMoves);

        var detachedInput = new RecordingInputSimulator();
        detachedInput.SetKeyDown(0x01, true);
        controller.Process(GameSnapshotTestSupport.CreateContext(GameSnapshot.Detached, detachedInput));
        Assert.Empty(detachedInput.MouseMoves);
    }
}

public sealed class AimHelperControllerTests
{
    [Fact]
    public void Process_moves_toward_best_candidate_in_fov()
    {
        var input = new RecordingInputSimulator();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var candidate = new AimCandidate(
            new PlayerId(2),
            BoneId.Head,
            new Vector3(10f, 0f, 0f),
            1.5f);
        var snapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            aimHelper: new AimHelperState([candidate]));

        var keybindMatcher = new Mock<IKeybindMatcher>();
        var controller = new AimHelperController(
            new FixedProjector(1000f, 600f),
            new FixedViewport(1920, 1080),
            keybindMatcher.Object);

        controller.Process(GameSnapshotTestSupport.CreateContext(snapshot, input));

        Assert.Single(input.MouseMoves);
        Assert.Equal(40, input.MouseMoves[0].X);
        Assert.Equal(60, input.MouseMoves[0].Y);
    }

    [Fact]
    public void Process_requires_activation_key_when_configured()
    {
        var input = new RecordingInputSimulator();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var candidate = new AimCandidate(
            new PlayerId(2),
            BoneId.Head,
            new Vector3(10f, 0f, 0f),
            1.5f);
        var snapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            aimHelper: new AimHelperState([candidate]));

        var keybindMatcher = new Mock<IKeybindMatcher>();
        keybindMatcher.Setup(m => m.ParseKey("MOUSE5")).Returns(new KeyCode(0x06));

        var controller = new AimHelperController(
            new FixedProjector(1000f, 600f),
            new FixedViewport(1920, 1080),
            keybindMatcher.Object);

        var context = GameSnapshotTestSupport.CreateContext(
            snapshot,
            input,
            keybinds: new GlobalKeybinds { AimHelperActivationKey = "MOUSE5" });

        controller.Process(context);
        Assert.Empty(input.MouseMoves);

        input.SetKeyDown(0x06, true);
        controller.Process(context);
        Assert.Single(input.MouseMoves);
    }
}
