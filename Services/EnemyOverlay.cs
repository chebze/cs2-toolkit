using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Models;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Services;

public sealed class EnemyOverlay : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly EnemyLastSeenTracker _lastSeenTracker;
    private readonly EnemyEspState _espState;
    private readonly ViewMatrixHolder _viewMatrixHolder;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly OverlayStyleState _overlayStyle;
    private readonly ILogger<EnemyOverlay> _logger;
    private OverlayLayer? _layer;

    public EnemyOverlay(
        ToolkitEventBus eventBus,
        EnemyLastSeenTracker lastSeenTracker,
        EnemyEspState espState,
        ViewMatrixHolder viewMatrixHolder,
        ScreenOverlayManager overlayManager,
        OverlayStyleState overlayStyle,
        ILogger<EnemyOverlay> logger)
    {
        _eventBus = eventBus;
        _lastSeenTracker = lastSeenTracker;
        _espState = espState;
        _viewMatrixHolder = viewMatrixHolder;
        _overlayManager = overlayManager;
        _overlayStyle = overlayStyle;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("enemy-last-seen", zIndex: 100);
        _eventBus.OnMemoryRead += OnMemoryRead;
        _layer.QueueDraw(DrawEsp);
        _logger.LogInformation("EnemyOverlay subscribed to OnMemoryRead");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnMemoryRead -= OnMemoryRead;
        _layer?.ClearQueue();
        return Task.CompletedTask;
    }

    private void OnMemoryRead(object? sender, MemoryReadEventArgs e)
    {
        if (_layer is null || _espState.Mode == EnemyEspMode.Disabled || !e.State.IsInMatch)
            return;

        _overlayManager.EnsureOnTop();
    }

    private void DrawEsp(Graphics graphics)
    {
        var mode = _espState.Mode;
        if (mode == EnemyEspMode.Disabled)
            return;

        var esp = _overlayStyle.Settings.EnemyEsp;
        var bounds = GameWindowHelper.GetTargetBounds();
        var skeletonColor = DrawHelper.ParseColor(esp.SkeletonColor, Color.OrangeRed);
        var boxColor = DrawHelper.ParseColor(esp.BoundingBoxColor, skeletonColor);
        var snapshots = mode switch
        {
            EnemyEspMode.Full => _lastSeenTracker.CopyLiveSnapshots(),
            EnemyEspMode.LastSeen => _lastSeenTracker.CopyDrawableSnapshots(),
            _ => []
        };

        if (snapshots.Count == 0)
            return;

        Span<float> viewMatrix = stackalloc float[16];
        _viewMatrixHolder.CopyTo(viewMatrix);

        SkeletonDrawer.DrawLastSeen(
            graphics,
            snapshots,
            viewMatrix,
            bounds.Width,
            bounds.Height,
            skeletonColor,
            esp.SkeletonLineWidth);

        foreach (var snapshot in snapshots)
        {
            if (esp.ShowBoundingBox)
                DrawBoundingBox(graphics, snapshot, viewMatrix, bounds.Width, bounds.Height, boxColor);

            if (esp.ShowPlayerName || esp.ShowPlayerHealth)
                DrawPlayerInfo(graphics, snapshot, viewMatrix, bounds.Width, bounds.Height, esp);
        }
    }

    private static void DrawBoundingBox(
        Graphics graphics,
        EnemyLastSeenSnapshot snapshot,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        Color color)
    {
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var any = false;

        foreach (var bone in snapshot.Bones)
        {
            if (!bone.IsValid)
                continue;

            if (!WorldToScreenHelper.TryProject(bone, viewMatrix, screenWidth, screenHeight, out var screen))
                continue;

            any = true;
            minX = Math.Min(minX, screen.X);
            minY = Math.Min(minY, screen.Y);
            maxX = Math.Max(maxX, screen.X);
            maxY = Math.Max(maxY, screen.Y);
        }

        if (!any)
            return;

        var padding = 4f;
        using var pen = new Pen(color, 1.5f);
        graphics.DrawRectangle(pen, minX - padding, minY - padding, maxX - minX + padding * 2f, maxY - minY + padding * 2f);
    }

    private static void DrawPlayerInfo(
        Graphics graphics,
        EnemyLastSeenSnapshot snapshot,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        EnemyEspProfileOptions esp)
    {
        if (!snapshot.Bones[PlayerBones.Head].IsValid)
            return;

        if (!WorldToScreenHelper.TryProject(
                snapshot.Bones[PlayerBones.Head],
                viewMatrix,
                screenWidth,
                screenHeight,
                out var head))
            return;

        var parts = new List<string>();
        if (esp.ShowPlayerName && !string.IsNullOrWhiteSpace(snapshot.Name))
            parts.Add(snapshot.Name);
        if (esp.ShowPlayerHealth)
            parts.Add($"{snapshot.Health} HP");

        if (parts.Count == 0)
            return;

        var text = string.Join(" · ", parts);
        using var font = new Font("Segoe UI", 11f, FontStyle.Bold);
        using var brush = new SolidBrush(DrawHelper.ParseColor(esp.SkeletonColor, Color.White));
        graphics.DrawString(text, font, brush, head.X, head.Y - 18f);
    }
}
