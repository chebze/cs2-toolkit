using System.Numerics;
using System.Text.RegularExpressions;

namespace Cs2Toolkit.Maps;

public sealed class MapCollisionMesh
{
    public required string MapName { get; init; }
    public required Vector3[] Vertices { get; init; }
    public required int[] Indices { get; init; }
    public int TriangleCount => Indices.Length / 3;
}

public sealed class MapRaycastIndex
{
    private const float MinCellSize = 64f;
    private const int TargetCellsPerAxis = 64;

    private readonly Vector3[] _vertices;
    private readonly int[] _indices;
    private readonly Vector3 _boundsMin;
    private readonly float _cellSize;
    private readonly int _sizeX;
    private readonly int _sizeY;
    private readonly int _sizeZ;
    private readonly List<int>[] _cells;
    private readonly int[] _visitStamps;
    private int _visitGeneration;

    public MapRaycastIndex(MapCollisionMesh mesh)
    {
        _vertices = mesh.Vertices;
        _indices = mesh.Indices;
        (_boundsMin, _cellSize, _sizeX, _sizeY, _sizeZ, _cells) = BuildSpatialGrid(mesh);
        _visitStamps = new int[mesh.TriangleCount];
    }

    public bool HasLineOfSight(Vector3 start, Vector3 end)
    {
        var direction = end - start;
        var distance = direction.Length();
        if (distance <= 0.01f)
            return true;

        direction /= distance;
        const float epsilon = 0.1f;

        return !TryRaycastInternal(start, direction, distance, out var hitDistance, out _)
               || hitDistance >= distance - epsilon;
    }

    public bool TryRaycast(
        Vector3 start,
        Vector3 direction,
        float maxDistance,
        out Vector3 hitPoint,
        out Vector3 normal,
        out float distance)
    {
        hitPoint = default;
        normal = default;
        distance = maxDistance;

        if (!TryRaycastInternal(start, direction, maxDistance, out var closest, out var closestNormal))
            return false;

        distance = closest;
        hitPoint = start + Vector3.Normalize(direction) * closest;
        normal = closestNormal;
        return true;
    }

    private bool TryRaycastInternal(
        Vector3 start,
        Vector3 direction,
        float maxDistance,
        out float distance,
        out Vector3 normal)
    {
        distance = maxDistance;
        normal = default;

        var length = direction.Length();
        if (length <= 1e-6f || maxDistance <= 0f)
            return false;

        direction /= length;
        var closest = maxDistance;
        var found = false;
        Vector3 closestNormal = default;

        _visitGeneration++;
        if (_visitGeneration == int.MaxValue)
        {
            Array.Clear(_visitStamps);
            _visitGeneration = 1;
        }

        TestCandidateTriangles(
            start,
            direction,
            maxDistance,
            _visitGeneration,
            ref closest,
            ref closestNormal,
            ref found);

        if (!found)
            return false;

        distance = closest;
        normal = closestNormal;
        return true;
    }

    private void TestCandidateTriangles(
        Vector3 origin,
        Vector3 direction,
        float maxDistance,
        int visitGeneration,
        ref float closest,
        ref Vector3 closestNormal,
        ref bool found)
    {
        if (_cells.Length == 0)
            return;

        var end = origin + direction * maxDistance;
        var minX = MathF.Min(origin.X, end.X);
        var minY = MathF.Min(origin.Y, end.Y);
        var minZ = MathF.Min(origin.Z, end.Z);
        var maxX = MathF.Max(origin.X, end.X);
        var maxY = MathF.Max(origin.Y, end.Y);
        var maxZ = MathF.Max(origin.Z, end.Z);

        var startCellX = ClampCellX((int)MathF.Floor((minX - _boundsMin.X) / _cellSize));
        var startCellY = ClampCellY((int)MathF.Floor((minY - _boundsMin.Y) / _cellSize));
        var startCellZ = ClampCellZ((int)MathF.Floor((minZ - _boundsMin.Z) / _cellSize));
        var endCellX = ClampCellX((int)MathF.Floor((maxX - _boundsMin.X) / _cellSize));
        var endCellY = ClampCellY((int)MathF.Floor((maxY - _boundsMin.Y) / _cellSize));
        var endCellZ = ClampCellZ((int)MathF.Floor((maxZ - _boundsMin.Z) / _cellSize));

        var invCell = 1f / _cellSize;
        var currentX = (int)MathF.Floor((origin.X - _boundsMin.X) * invCell);
        var currentY = (int)MathF.Floor((origin.Y - _boundsMin.Y) * invCell);
        var currentZ = (int)MathF.Floor((origin.Z - _boundsMin.Z) * invCell);
        currentX = ClampCellX(currentX);
        currentY = ClampCellY(currentY);
        currentZ = ClampCellZ(currentZ);

        var stepX = direction.X > 0f ? 1 : direction.X < 0f ? -1 : 0;
        var stepY = direction.Y > 0f ? 1 : direction.Y < 0f ? -1 : 0;
        var stepZ = direction.Z > 0f ? 1 : direction.Z < 0f ? -1 : 0;

        var nextBoundaryX = stepX > 0
            ? _boundsMin.X + (currentX + 1) * _cellSize
            : _boundsMin.X + currentX * _cellSize;
        var nextBoundaryY = stepY > 0
            ? _boundsMin.Y + (currentY + 1) * _cellSize
            : _boundsMin.Y + currentY * _cellSize;
        var nextBoundaryZ = stepZ > 0
            ? _boundsMin.Z + (currentZ + 1) * _cellSize
            : _boundsMin.Z + currentZ * _cellSize;

        var tMaxX = stepX == 0 ? float.PositiveInfinity : (nextBoundaryX - origin.X) / direction.X;
        var tMaxY = stepY == 0 ? float.PositiveInfinity : (nextBoundaryY - origin.Y) / direction.Y;
        var tMaxZ = stepZ == 0 ? float.PositiveInfinity : (nextBoundaryZ - origin.Z) / direction.Z;

        var tDeltaX = stepX == 0 ? float.PositiveInfinity : _cellSize / MathF.Abs(direction.X);
        var tDeltaY = stepY == 0 ? float.PositiveInfinity : _cellSize / MathF.Abs(direction.Y);
        var tDeltaZ = stepZ == 0 ? float.PositiveInfinity : _cellSize / MathF.Abs(direction.Z);

        var steps = 0;
        var maxSteps = Math.Abs(endCellX - startCellX)
            + Math.Abs(endCellY - startCellY)
            + Math.Abs(endCellZ - startCellZ)
            + 8;

        while (steps++ <= maxSteps)
        {
            var flat = ToFlat(currentX, currentY, currentZ);
            var bucket = _cells[flat];
            for (var i = 0; i < bucket.Count; i++)
            {
                var triangleOffset = bucket[i];
                var triangleIndex = triangleOffset / 3;
                if (_visitStamps[triangleIndex] == visitGeneration)
                    continue;

                _visitStamps[triangleIndex] = visitGeneration;

                var v0 = _vertices[_indices[triangleOffset]];
                var v1 = _vertices[_indices[triangleOffset + 1]];
                var v2 = _vertices[_indices[triangleOffset + 2]];

                if (!IntersectsTriangle(origin, direction, maxDistance, v0, v1, v2, out var hitDistance))
                    continue;

                if (hitDistance >= closest)
                    continue;

                closest = hitDistance;
                closestNormal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
                if (Vector3.Dot(closestNormal, direction) > 0f)
                    closestNormal = -closestNormal;

                found = true;
            }

            if (currentX == endCellX && currentY == endCellY && currentZ == endCellZ)
                break;

            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    if (tMaxX > maxDistance)
                        break;

                    currentX += stepX;
                    if (currentX < 0 || currentX >= _sizeX)
                        break;

                    tMaxX += tDeltaX;
                }
                else
                {
                    if (tMaxZ > maxDistance)
                        break;

                    currentZ += stepZ;
                    if (currentZ < 0 || currentZ >= _sizeZ)
                        break;

                    tMaxZ += tDeltaZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    if (tMaxY > maxDistance)
                        break;

                    currentY += stepY;
                    if (currentY < 0 || currentY >= _sizeY)
                        break;

                    tMaxY += tDeltaY;
                }
                else
                {
                    if (tMaxZ > maxDistance)
                        break;

                    currentZ += stepZ;
                    if (currentZ < 0 || currentZ >= _sizeZ)
                        break;

                    tMaxZ += tDeltaZ;
                }
            }
        }
    }

    private static (Vector3 BoundsMin, float CellSize, int SizeX, int SizeY, int SizeZ, List<int>[] Cells) BuildSpatialGrid(
        MapCollisionMesh mesh)
    {
        if (mesh.TriangleCount == 0)
            return (Vector3.Zero, MinCellSize, 0, 0, 0, []);

        var boundsMin = new Vector3(float.PositiveInfinity);
        var boundsMax = new Vector3(float.NegativeInfinity);

        foreach (var vertex in mesh.Vertices)
        {
            boundsMin = Vector3.Min(boundsMin, vertex);
            boundsMax = Vector3.Max(boundsMax, vertex);
        }

        var extent = boundsMax - boundsMin;
        var maxExtent = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z));
        var cellSize = MathF.Max(MinCellSize, maxExtent / TargetCellsPerAxis);

        var sizeX = Math.Max(1, (int)MathF.Ceiling(extent.X / cellSize) + 1);
        var sizeY = Math.Max(1, (int)MathF.Ceiling(extent.Y / cellSize) + 1);
        var sizeZ = Math.Max(1, (int)MathF.Ceiling(extent.Z / cellSize) + 1);

        var cells = new List<int>[sizeX * sizeY * sizeZ];
        for (var i = 0; i < cells.Length; i++)
            cells[i] = [];

        for (var triangleOffset = 0; triangleOffset < mesh.Indices.Length; triangleOffset += 3)
        {
            var v0 = mesh.Vertices[mesh.Indices[triangleOffset]];
            var v1 = mesh.Vertices[mesh.Indices[triangleOffset + 1]];
            var v2 = mesh.Vertices[mesh.Indices[triangleOffset + 2]];

            var triMin = Vector3.Min(v0, Vector3.Min(v1, v2));
            var triMax = Vector3.Max(v0, Vector3.Max(v1, v2));

            var minCellX = ClampCell((int)MathF.Floor((triMin.X - boundsMin.X) / cellSize), sizeX);
            var minCellY = ClampCell((int)MathF.Floor((triMin.Y - boundsMin.Y) / cellSize), sizeY);
            var minCellZ = ClampCell((int)MathF.Floor((triMin.Z - boundsMin.Z) / cellSize), sizeZ);
            var maxCellX = ClampCell((int)MathF.Floor((triMax.X - boundsMin.X) / cellSize), sizeX);
            var maxCellY = ClampCell((int)MathF.Floor((triMax.Y - boundsMin.Y) / cellSize), sizeY);
            var maxCellZ = ClampCell((int)MathF.Floor((triMax.Z - boundsMin.Z) / cellSize), sizeZ);

            for (var z = minCellZ; z <= maxCellZ; z++)
            {
                for (var y = minCellY; y <= maxCellY; y++)
                {
                    for (var x = minCellX; x <= maxCellX; x++)
                        cells[x + sizeX * (y + sizeY * z)].Add(triangleOffset);
                }
            }
        }

        return (boundsMin, cellSize, sizeX, sizeY, sizeZ, cells);
    }

    private int ToFlat(int x, int y, int z) => x + _sizeX * (y + _sizeY * z);

    private int ClampCellX(int value) => ClampCell(value, _sizeX);
    private int ClampCellY(int value) => ClampCell(value, _sizeY);
    private int ClampCellZ(int value) => ClampCell(value, _sizeZ);

    private static int ClampCell(int value, int size) => Math.Clamp(value, 0, size - 1);

    private static bool IntersectsTriangle(
        Vector3 origin,
        Vector3 direction,
        float maxDistance,
        Vector3 v0,
        Vector3 v1,
        Vector3 v2,
        out float distance)
    {
        distance = 0;
        var edge1 = v1 - v0;
        var edge2 = v2 - v0;
        var pvec = Vector3.Cross(direction, edge2);
        var det = Vector3.Dot(edge1, pvec);
        if (MathF.Abs(det) < 1e-8f)
            return false;

        var invDet = 1f / det;
        var tvec = origin - v0;
        var u = Vector3.Dot(tvec, pvec) * invDet;
        if (u < 0f || u > 1f)
            return false;

        var qvec = Vector3.Cross(tvec, edge1);
        var v = Vector3.Dot(direction, qvec) * invDet;
        if (v < 0f || u + v > 1f)
            return false;

        distance = Vector3.Dot(edge2, qvec) * invDet;
        return distance > 0f && distance <= maxDistance;
    }
}

public sealed class MapVisibilityChecker
{
    private readonly object _lock = new();
    private readonly Dictionary<string, MapRaycastIndex> _indices = new(StringComparer.OrdinalIgnoreCase);
    private string? _activeMapName;

    public bool IsReady => _indices.Count > 0;

    public int LoadedMapCount
    {
        get
        {
            lock (_lock)
                return _indices.Count;
        }
    }

    public void RegisterMap(MapCollisionMesh mesh)
    {
        if (mesh.TriangleCount == 0)
            return;

        lock (_lock)
            _indices[mesh.MapName] = new MapRaycastIndex(mesh);
    }

    public void SetActiveMap(string? mapName)
    {
        lock (_lock)
            _activeMapName = NormalizeMapName(mapName);
    }

    public bool TryHasLineOfSight(Vector3 start, Vector3 end)
    {
        MapRaycastIndex? index;
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(_activeMapName)
                || !_indices.TryGetValue(_activeMapName, out index))
                return true;
        }

        return index.HasLineOfSight(start, end);
    }

    public bool TryRaycast(
        Vector3 start,
        Vector3 direction,
        float maxDistance,
        out Vector3 hitPoint,
        out Vector3 normal,
        out float distance)
    {
        hitPoint = default;
        normal = default;
        distance = maxDistance;

        MapRaycastIndex? index;
        lock (_lock)
        {
            if (!TryResolveRaycastIndex(out index) || index is null)
                return false;
        }

        return index.TryRaycast(start, direction, maxDistance, out hitPoint, out normal, out distance);
    }

    private bool TryResolveRaycastIndex(out MapRaycastIndex? index)
    {
        index = null;

        if (!string.IsNullOrWhiteSpace(_activeMapName)
            && _indices.TryGetValue(_activeMapName, out index))
            return true;

        if (_indices.Count == 1)
        {
            index = _indices.Values.First();
            return true;
        }

        if (string.IsNullOrWhiteSpace(_activeMapName))
            return false;

        foreach (var pair in _indices)
        {
            if (pair.Key.Contains(_activeMapName, StringComparison.OrdinalIgnoreCase)
                || _activeMapName.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                index = pair.Value;
                return true;
            }
        }

        return false;
    }

    public static string NormalizeMapName(string? mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName))
            return string.Empty;

        var printable = new string(mapName.Where(static c => c >= 32 && c <= 126).ToArray());
        var normalized = printable.Trim().Replace('\\', '/');
        var fileName = Path.GetFileNameWithoutExtension(normalized);

        var match = MapNamePattern.Match(fileName);
        if (match.Success)
            return match.Value.ToLowerInvariant();

        match = MapNamePattern.Match(normalized);
        return match.Success ? match.Value.ToLowerInvariant() : fileName;
    }

    private static readonly Regex MapNamePattern = new(
        @"(?:cs_|de_|ar_)[a-z0-9_]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
}
