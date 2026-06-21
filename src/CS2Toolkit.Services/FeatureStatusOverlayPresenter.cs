using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class FeatureStatusOverlayPresenter : IOverlayPresenter
{
    private readonly IFeatureState _state;

    public FeatureStatusOverlayPresenter(IFeatureState state) => _state = state;

    public string LayerName => "feature-status";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        if (!snapshot.IsAttached || !snapshot.IsInMatch)
            return [];

        var lines = new List<string>();
        AppendLine(lines, "RCS", _state.IsEnabled(FeatureIds.Rcs));
        AppendLine(lines, "TB", _state.IsEnabled(FeatureIds.Triggerbot));
        lines.Add($"ESP: {_state.EnemyEspMode}");
        AppendLine(lines, "Sound", _state.IsEnabled(FeatureIds.SoundEsp));
        AppendLine(lines, "Tracers", _state.IsEnabled(FeatureIds.BulletTracers));
        AppendLine(lines, "Aim", _state.IsEnabled(FeatureIds.AimHelper));

        if (_state.TriggerbotAutoStopEnabled)
            lines.Add("TB auto-stop: on");

        var commands = new List<DrawCommand>();
        var y = 16f;
        foreach (var line in lines)
        {
            commands.Add(new TextDrawCommand(16f, y, line, 0xE6FFFFFF, FontSize: 11f, ZIndex: 900));
            y += 14f;
        }

        return commands;
    }

    private static void AppendLine(List<string> lines, string label, bool enabled) =>
        lines.Add($"{label}: {(enabled ? "on" : "off")}");
}
