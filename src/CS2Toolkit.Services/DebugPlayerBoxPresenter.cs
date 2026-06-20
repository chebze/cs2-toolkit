using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

public sealed class DebugPlayerBoxPresenter : IOverlayPresenter
{
    private const float EstimatedPlayerHeight = 72f;
    private const int ZIndex = 100;

    private readonly ToolkitHostSettings _options;

    public DebugPlayerBoxPresenter(IOptions<ToolkitHostSettings> options) =>
        _options = options.Value;

    public string LayerName => "debug-player-boxes";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        if (!_options.ShowDebugPlayerBoxes || !snapshot.IsInMatch)
            return [];

        var commands = new List<DrawCommand>();
        foreach (var player in snapshot.Players)
        {
            if (!player.IsAlive || player.IsLocalPlayer || player.WorldPosition is not { } feet)
                continue;

            var head = new Vector3(feet.X, feet.Y, feet.Z + EstimatedPlayerHeight);
            if (!projector.TryProject(feet, snapshot.ViewMatrix, screenWidth, screenHeight, out var feetX, out var feetY)
                || !projector.TryProject(head, snapshot.ViewMatrix, screenWidth, screenHeight, out var headX, out var headY))
            {
                continue;
            }

            var top = MathF.Min(headY, feetY);
            var bottom = MathF.Max(headY, feetY);
            var height = MathF.Max(8f, bottom - top);
            var width = MathF.Max(6f, height * 0.45f);
            var color = player.Team switch
            {
                Team.Terrorist => 0xFFFF8C00,
                Team.CounterTerrorist => 0xFF1E90FF,
                _ => 0xFFFFFFFF
            };

            commands.Add(new RectDrawCommand(
                feetX - width * 0.5f,
                top,
                width,
                height,
                color,
                StrokeWidth: 2f,
                Filled: false,
                ZIndex: ZIndex));

            commands.Add(new TextDrawCommand(
                feetX - width * 0.5f,
                top - 14f,
                $"{player.Name} ({player.Health})",
                color,
                FontSize: 10f,
                ZIndex: ZIndex + 1));
        }

        return commands;
    }
}
