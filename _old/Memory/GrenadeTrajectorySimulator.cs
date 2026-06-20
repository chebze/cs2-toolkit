using System.Numerics;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Maps;
using ModelVector3 = Cs2Toolkit.Models.Vector3;

namespace Cs2Toolkit.Memory;

public sealed class GrenadeTrajectorySimulator
{
    private readonly GrenadeOptions _options;

    public GrenadeTrajectorySimulator(GrenadeOptions options) => _options = options;

    public bool TrySimulate(
        MapVisibilityChecker mapChecker,
        Vector3 start,
        Vector3 velocity,
        out List<ModelVector3> points,
        out List<List<ModelVector3>> segments,
        out List<ModelVector3> bouncePoints,
        out ModelVector3 landingPoint,
        out int bounceCount)
    {
        points = [];
        segments = [];
        bouncePoints = [];
        landingPoint = default;
        bounceCount = 0;

        if (!IsFinite(start) || !IsFinite(velocity) || velocity.LengthSquared() < 100f)
            return false;

        var currentSegment = new List<ModelVector3>();
        segments.Add(currentSegment);

        var position = start;
        var currentVelocity = velocity;
        RecordPoint(points, currentSegment, position, force: true);

        var tickInterval = _options.TickIntervalSeconds;
        var gravity = _options.Gravity;
        var elasticity = _options.BounceElasticity;
        var surfaceOffset = _options.SurfaceOffset;
        var substeps = Math.Max(1, _options.RaycastSubSteps);

        for (var tick = 0; tick < _options.MaxSimulationTicks; tick++)
        {
            var newVelocityZ = currentVelocity.Z - gravity * tickInterval;
            var move = new Vector3(
                currentVelocity.X * tickInterval,
                currentVelocity.Y * tickInterval,
                ((currentVelocity.Z + newVelocityZ) * 0.5f) * tickInterval);
            currentVelocity.Z = newVelocityZ;

            var moveLength = move.Length();
            if (moveLength <= 1e-4f)
                break;

            var subMove = move / substeps;
            var hitThisTick = false;

            for (var step = 0; step < substeps; step++)
            {
                var stepLength = subMove.Length();
                if (stepLength <= 1e-5f)
                    continue;

                var stepDirection = Vector3.Normalize(subMove);
                var traceDistance = stepLength + _options.RaycastSkin;

                if (mapChecker.TryRaycast(position, stepDirection, traceDistance, out var hitPoint, out var normal, out var hitDistance)
                    && hitDistance <= stepLength + _options.RaycastSkin * 0.5f)
                {
                    position = hitPoint - normal * surfaceOffset;
                    RecordPoint(points, currentSegment, position, force: true);
                    bouncePoints.Add(ToModel(position));
                    landingPoint = ToModel(position);
                    bounceCount++;
                    hitThisTick = true;

                    currentVelocity = Reflect(currentVelocity, normal) * elasticity;

                    if (bounceCount >= _options.MaxBounces
                        || currentVelocity.LengthSquared() < _options.StopVelocityThreshold * _options.StopVelocityThreshold)
                    {
                        FinalizeLanding(mapChecker, position, ref landingPoint);
                        RecordPoint(points, currentSegment, ToNumeric(landingPoint), force: true);
                        return points.Count >= 2;
                    }

                    currentSegment = [ToModel(position)];
                    segments.Add(currentSegment);

                    var separation = Math.Max(surfaceOffset * 2f, 2f);
                    if (currentVelocity.LengthSquared() > 1f)
                        position += Vector3.Normalize(currentVelocity) * separation;

                    RecordPoint(points, currentSegment, position, force: true);
                    break;
                }

                position += subMove;
                RecordPoint(points, currentSegment, position, force: true);
            }

            if (!hitThisTick)
                landingPoint = ToModel(position);

            if (currentVelocity.LengthSquared() < _options.StopVelocityThreshold * _options.StopVelocityThreshold)
                break;
        }

        if (points.Count > 0)
        {
            FinalizeLanding(mapChecker, position, ref landingPoint);
            RecordPoint(points, currentSegment, ToNumeric(landingPoint), force: true);
        }

        return points.Count >= 2;
    }

    public static bool TryComputeThrowState(
        Vector3 eyePosition,
        float pitchDegrees,
        float yawDegrees,
        float throwStrength,
        ushort weaponId,
        Vector3 playerVelocity,
        GrenadeOptions options,
        out Vector3 start,
        out Vector3 velocity)
    {
        start = default;
        velocity = default;

        if (!IsFinite(eyePosition))
            return false;

        pitchDegrees = AdjustThrowPitch(pitchDegrees);
        var power = Math.Clamp(throwStrength, 0f, 1f);
        var baseSpeed = Models.GameOffsets.GetGrenadeThrowVelocity(weaponId) * 0.9f;
        var speed = baseSpeed * (options.MinThrowSpeedScale + options.MaxThrowSpeedScale * power);
        var forward = DirectionFromAngles(pitchDegrees, yawDegrees);

        start = eyePosition;
        start.Z += power * 12f - 12f;
        start += forward * (options.ThrowForwardTraceUnits - options.ThrowStartPullbackUnits);

        velocity = forward * speed + ComputeInheritedPlayerVelocity(forward, playerVelocity, options.PlayerVelocityScale);
        return velocity.LengthSquared() >= 100f;
    }

    public static bool TryBuildThrowFromPosition(
        Vector3 throwPosition,
        float pitchDegrees,
        float yawDegrees,
        float throwStrength,
        ushort weaponId,
        Vector3 playerVelocity,
        GrenadeOptions options,
        out Vector3 start,
        out Vector3 velocity)
    {
        start = default;
        velocity = default;

        if (!IsFinite(throwPosition))
            return false;

        pitchDegrees = AdjustThrowPitch(pitchDegrees);
        var power = Math.Clamp(throwStrength, 0f, 1f);
        var baseSpeed = Models.GameOffsets.GetGrenadeThrowVelocity(weaponId) * 0.9f;
        var speed = baseSpeed * (options.MinThrowSpeedScale + options.MaxThrowSpeedScale * power);
        var forward = DirectionFromAngles(pitchDegrees, yawDegrees);

        start = throwPosition;
        velocity = forward * speed + ComputeInheritedPlayerVelocity(forward, playerVelocity, options.PlayerVelocityScale);
        return velocity.LengthSquared() >= 100f;
    }

    public static Vector3 ComputeInheritedPlayerVelocity(Vector3 throwDirection, Vector3 playerVelocity, float scale)
    {
        var horizontalAim = new Vector3(throwDirection.X, throwDirection.Y, 0f);
        if (horizontalAim.LengthSquared() > 1e-4f)
            horizontalAim = Vector3.Normalize(horizontalAim);

        var horizontalVelocity = new Vector3(playerVelocity.X, playerVelocity.Y, 0f);
        var forwardRunSpeed = Vector3.Dot(horizontalVelocity, horizontalAim);

        return horizontalAim * (forwardRunSpeed * scale)
            + new Vector3(0f, 0f, playerVelocity.Z * scale);
    }

    public static float AdjustThrowPitch(float pitchDegrees)
    {
        if (pitchDegrees > 90f)
            pitchDegrees -= 360f;
        else if (pitchDegrees < -90f)
            pitchDegrees += 360f;

        return pitchDegrees - (90f - MathF.Abs(pitchDegrees)) * 10f / 90f;
    }

    public static Vector3 ComputeThrowVelocity(
        float pitchDegrees,
        float yawDegrees,
        float throwStrength,
        Vector3 playerVelocity,
        float baseThrowVelocity,
        GrenadeOptions options)
    {
        pitchDegrees = AdjustThrowPitch(pitchDegrees);
        var power = Math.Clamp(throwStrength, 0f, 1f);
        var speed = baseThrowVelocity * 0.9f * (options.MinThrowSpeedScale + options.MaxThrowSpeedScale * power);
        var forward = DirectionFromAngles(pitchDegrees, yawDegrees);
        return forward * speed + ComputeInheritedPlayerVelocity(forward, playerVelocity, options.PlayerVelocityScale);
    }

    public static Vector3 DirectionFromAngles(float pitchDegrees, float yawDegrees)
    {
        var pitch = DegreesToRadians(pitchDegrees);
        var yaw = DegreesToRadians(yawDegrees);
        var cosPitch = MathF.Cos(pitch);

        return Vector3.Normalize(new Vector3(
            cosPitch * MathF.Cos(yaw),
            cosPitch * MathF.Sin(yaw),
            -MathF.Sin(pitch)));
    }

    public static bool IsPlausibleTrajectory(
        IReadOnlyList<ModelVector3> points,
        float minHorizontalTravelUnits)
    {
        if (points.Count < 3)
            return false;

        var start = points[0];
        var end = points[^1];
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var horizontalTravel = MathF.Sqrt(dx * dx + dy * dy);
        if (horizontalTravel < minHorizontalTravelUnits)
            return false;

        var totalLength = 0f;
        for (var i = 1; i < points.Count; i++)
            totalLength += SegmentLength(points[i - 1], points[i]);

        var chord = SegmentLength(start, end);
        if (chord <= 1f)
            return false;

        var maxDeviation = 0f;
        for (var i = 1; i < points.Count - 1; i++)
            maxDeviation = MathF.Max(maxDeviation, DistancePointToSegment(points[i], start, end));

        return maxDeviation >= 4f || totalLength >= chord * 1.08f;
    }

    private void FinalizeLanding(MapVisibilityChecker mapChecker, Vector3 position, ref ModelVector3 landingPoint)
    {
        var down = new Vector3(0f, 0f, -1f);
        if (mapChecker.TryRaycast(position, down, 2048f, out var groundHit, out _, out _))
            landingPoint = ToModel(groundHit);
        else
            landingPoint = ToModel(position);
    }

    private static float SegmentLength(ModelVector3 a, ModelVector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static float DistancePointToSegment(ModelVector3 point, ModelVector3 start, ModelVector3 end)
    {
        var abX = end.X - start.X;
        var abY = end.Y - start.Y;
        var abZ = end.Z - start.Z;
        var abLenSq = abX * abX + abY * abY + abZ * abZ;
        if (abLenSq <= 1e-4f)
            return SegmentLength(point, start);

        var apX = point.X - start.X;
        var apY = point.Y - start.Y;
        var apZ = point.Z - start.Z;
        var t = Math.Clamp((apX * abX + apY * abY + apZ * abZ) / abLenSq, 0f, 1f);

        var closestX = start.X + abX * t;
        var closestY = start.Y + abY * t;
        var closestZ = start.Z + abZ * t;
        var dx = point.X - closestX;
        var dy = point.Y - closestY;
        var dz = point.Z - closestZ;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private void RecordPoint(List<ModelVector3> points, List<ModelVector3> segment, Vector3 position, bool force)
    {
        if (!force && segment.Count > 0)
        {
            var last = segment[^1];
            var dx = position.X - last.X;
            var dy = position.Y - last.Y;
            var dz = position.Z - last.Z;
            var minSpacing = _options.MinPointSpacingUnits;
            if (dx * dx + dy * dy + dz * dz < minSpacing * minSpacing)
                return;
        }

        var model = ToModel(position);
        segment.Add(model);
        points.Add(model);
    }

    private static Vector3 Reflect(Vector3 velocity, Vector3 normal)
    {
        var dot = Vector3.Dot(velocity, normal);
        return velocity - 2f * dot * normal;
    }

    private static float DegreesToRadians(float degrees) => degrees * (MathF.PI / 180f);

    private static ModelVector3 ToModel(Vector3 vector) =>
        new(vector.X, vector.Y, vector.Z);

    private static Vector3 ToNumeric(ModelVector3 vector) =>
        new(vector.X, vector.Y, vector.Z);

    private static bool IsFinite(Vector3 vector) =>
        !float.IsNaN(vector.X) && !float.IsNaN(vector.Y) && !float.IsNaN(vector.Z)
        && !float.IsInfinity(vector.X) && !float.IsInfinity(vector.Y) && !float.IsInfinity(vector.Z);
}
