using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

internal static class EnemyEspDrawBuilder
{
    private const float HeadCircleScale = 0.45f;
    private const float BoundingBoxPadding = 4f;

    public static IReadOnlyList<DrawCommand> Build(
        IReadOnlyList<EspTarget> targets,
        EnemyEspProfileOptions options,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        if (targets.Count == 0)
            return [];

        var skeletonColor = OverlayColorParser.ParseArgb(options.SkeletonColor, 0xFFFF6B6B);
        var boxColor = OverlayColorParser.ParseArgb(options.BoundingBoxColor, skeletonColor);
        var commands = new List<DrawCommand>();

        foreach (var target in targets)
        {
            if (!target.Bones.HasValidSkeleton)
                continue;

            commands.AddRange(BuildSkeleton(target.Bones, skeletonColor, options.SkeletonLineWidth, projector, viewMatrix, screenWidth, screenHeight, zIndex));

            if (options.ShowBoundingBox)
                TryAddBoundingBox(commands, target.Bones, boxColor, projector, viewMatrix, screenWidth, screenHeight, zIndex);

            if (options.ShowPlayerName || options.ShowPlayerHealth)
                TryAddPlayerInfo(commands, target, skeletonColor, options, projector, viewMatrix, screenWidth, screenHeight, zIndex);
        }

        return commands;
    }

    private static IEnumerable<DrawCommand> BuildSkeleton(
        PlayerBones bones,
        uint color,
        float lineWidth,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        var commands = new List<DrawCommand>();

        if (bones.TryGetBone((int)BoneId.Head, out var headWorld)
            && projector.TryProject(headWorld, viewMatrix, screenWidth, screenHeight, out var headX, out var headY))
        {
            var radius = 5f;
            if (bones.TryGetBone((int)BoneId.Neck, out var neckWorld)
                && projector.TryProject(neckWorld, viewMatrix, screenWidth, screenHeight, out var neckX, out var neckY))
            {
                var dx = headX - neckX;
                var dy = headY - neckY;
                radius = MathF.Max(3f, MathF.Sqrt(dx * dx + dy * dy) * HeadCircleScale);
            }

            commands.Add(new CircleDrawCommand(headX, headY, radius, color, lineWidth, Filled: false, zIndex));
        }

        foreach (var (from, to) in PlayerBones.Connections)
        {
            if (!bones.TryGetBone(from, out var startWorld) || !bones.TryGetBone(to, out var endWorld))
                continue;

            if (startWorld.DistanceTo(endWorld) > PlayerBones.MaxConnectionWorldDistance)
                continue;

            if (!projector.TryProject(startWorld, viewMatrix, screenWidth, screenHeight, out var startX, out var startY)
                || !projector.TryProject(endWorld, viewMatrix, screenWidth, screenHeight, out var endX, out var endY))
            {
                continue;
            }

            commands.Add(new LineDrawCommand(startX, startY, endX, endY, color, lineWidth, zIndex));
        }

        return commands;
    }

    private static void TryAddBoundingBox(
        List<DrawCommand> commands,
        PlayerBones bones,
        uint color,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var any = false;

        foreach (var bone in bones.Bones)
        {
            if (!bone.IsValid)
                continue;

            if (!projector.TryProject(bone.Position, viewMatrix, screenWidth, screenHeight, out var x, out var y))
                continue;

            any = true;
            minX = MathF.Min(minX, x);
            minY = MathF.Min(minY, y);
            maxX = MathF.Max(maxX, x);
            maxY = MathF.Max(maxY, y);
        }

        if (!any)
            return;

        commands.Add(new RectDrawCommand(
            minX - BoundingBoxPadding,
            minY - BoundingBoxPadding,
            maxX - minX + BoundingBoxPadding * 2f,
            maxY - minY + BoundingBoxPadding * 2f,
            color,
            StrokeWidth: 1.5f,
            Filled: false,
            zIndex));
    }

    private static void TryAddPlayerInfo(
        List<DrawCommand> commands,
        EspTarget target,
        uint color,
        EnemyEspProfileOptions options,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        if (!target.Bones.TryGetBone((int)BoneId.Head, out var headWorld)
            || !projector.TryProject(headWorld, viewMatrix, screenWidth, screenHeight, out var headX, out var headY))
        {
            return;
        }

        var parts = new List<string>();
        if (options.ShowPlayerName && !string.IsNullOrWhiteSpace(target.Name))
            parts.Add(target.Name);
        if (options.ShowPlayerHealth)
            parts.Add($"{target.Health} HP");

        if (parts.Count == 0)
            return;

        commands.Add(new TextDrawCommand(
            headX,
            headY - 18f,
            string.Join(" · ", parts),
            color,
            FontSize: 11f,
            ZIndex: zIndex + 1));
    }
}
