using System.Numerics;

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
    private readonly Vector3[] _vertices;
    private readonly int[] _indices;

    public MapRaycastIndex(MapCollisionMesh mesh)
    {
        _vertices = mesh.Vertices;
        _indices = mesh.Indices;
    }

    public bool HasLineOfSight(Vector3 start, Vector3 end)
    {
        var direction = end - start;
        var distance = direction.Length();
        if (distance <= 0.01f)
            return true;

        direction /= distance;
        const float epsilon = 0.1f;

        for (var i = 0; i < _indices.Length; i += 3)
        {
            if (IntersectsTriangle(start, direction, distance,
                    _vertices[_indices[i]],
                    _vertices[_indices[i + 1]],
                    _vertices[_indices[i + 2]],
                    out var hitDistance)
                && hitDistance < distance - epsilon)
                return false;
        }

        return true;
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

        var length = direction.Length();
        if (length <= 1e-6f || maxDistance <= 0f)
            return false;

        direction /= length;
        var closest = maxDistance;
        var found = false;
        Vector3 closestNormal = default;

        for (var i = 0; i < _indices.Length; i += 3)
        {
            var v0 = _vertices[_indices[i]];
            var v1 = _vertices[_indices[i + 1]];
            var v2 = _vertices[_indices[i + 2]];

            if (!IntersectsTriangle(start, direction, maxDistance, v0, v1, v2, out var hitDistance))
                continue;

            if (hitDistance >= closest)
                continue;

            closest = hitDistance;
            closestNormal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
            if (Vector3.Dot(closestNormal, direction) > 0f)
                closestNormal = -closestNormal;

            found = true;
        }

        if (!found)
            return false;

        distance = closest;
        hitPoint = start + direction * closest;
        normal = closestNormal;
        return true;
    }

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

        var normalized = mapName.Trim().Replace('\\', '/');
        var fileName = Path.GetFileNameWithoutExtension(normalized);
        return fileName;
    }
}
