using System.Numerics;
using System.Text;
using Microsoft.Extensions.Logging;
using SteamDatabase.ValvePak;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.ResourceTypes.RubikonPhysics;

namespace CS2Toolkit.Game.Maps;

public sealed class MapPhysicsParser
{
    private readonly ILogger<MapPhysicsParser> _logger;

    public MapPhysicsParser(ILogger<MapPhysicsParser> logger) => _logger = logger;

    public MapCollisionMesh? LoadCachedMesh(string cachePath)
    {
        var mapName = Path.GetFileNameWithoutExtension(cachePath);
        return TryLoadCache(cachePath, mapName, out var cached) ? cached : null;
    }

    public MapCollisionMesh? ParseMapVpk(string vpkPath, string cacheDirectory, string? mapsDirectory = null)
    {
        var mapName = Path.GetFileNameWithoutExtension(vpkPath);
        if (mapName.EndsWith("_dir", StringComparison.OrdinalIgnoreCase))
            mapName = mapName[..^4];

        var cachePath = Path.Combine(cacheDirectory, $"{mapName}.mapmesh");
        if (TryLoadCache(cachePath, mapName, out var cached))
            return cached;

        MapCollisionMesh? mesh = null;
        try
        {
            using var package = new Package();
            package.Read(vpkPath);
            mesh = TryParsePhysicsFromPackage(package, mapName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open map VPK {VpkPath}", vpkPath);
        }

        if (mesh is null && mapsDirectory is not null)
        {
            var pak01Path = Path.GetFullPath(Path.Combine(mapsDirectory, "..", "pak01_dir.vpk"));
            if (File.Exists(pak01Path) && !string.Equals(pak01Path, vpkPath, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var pak01 = new Package();
                    pak01.Read(pak01Path);
                    mesh = TryParsePhysicsFromPackage(pak01, mapName);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to read fallback physics from {Pak01Path}", pak01Path);
                }
            }
        }

        if (mesh is null || mesh.TriangleCount == 0)
        {
            _logger.LogDebug("No parseable world_physics found for {MapName} in {VpkPath}", mapName, vpkPath);
            return null;
        }

        TrySaveCache(cachePath, mesh);
        return mesh;
    }

    private MapCollisionMesh? TryParsePhysicsFromPackage(Package package, string mapName)
    {
        foreach (var relativePath in new[]
                 {
                     $"maps/{mapName}/world_physics.vphys_c",
                     $"maps/{mapName}/world_physics.vmdl_c"
                 })
        {
            var entry = package.FindEntry(relativePath);
            if (entry is null)
                continue;

            package.ReadEntry(entry, out var data);
            var mesh = ParsePhysicsResource(mapName, relativePath, data);
            if (mesh is not null)
                return mesh;
        }

        foreach (var group in package.Entries)
        {
            if (group.Value is null)
                continue;

            foreach (var candidate in group.Value)
            {
                var fullPath = candidate.GetFullPath();
                if (!fullPath.Contains($"/{mapName}/", StringComparison.OrdinalIgnoreCase)
                    && !fullPath.Contains($"\\{mapName}\\", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!fullPath.EndsWith("world_physics.vphys_c", StringComparison.OrdinalIgnoreCase)
                    && !fullPath.EndsWith("world_physics.vmdl_c", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                package.ReadEntry(candidate, out var data);
                var mesh = ParsePhysicsResource(mapName, fullPath, data);
                if (mesh is not null)
                    return mesh;
            }
        }

        return null;
    }

    private MapCollisionMesh? ParsePhysicsResource(string mapName, string resourcePath, byte[] resourceBytes)
    {
        try
        {
            using var stream = new MemoryStream(resourceBytes);
            using var resource = new Resource { FileName = Path.GetFileName(resourcePath) };
            resource.Read(stream);

            PhysAggregateData? physData = resource.DataBlock as PhysAggregateData;
            if (physData is null && resource.DataBlock is Model model)
                physData = model.GetEmbeddedPhys();

            if (physData is null)
                return null;

            return BuildMeshFromPhysAggregate(mapName, physData);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse {ResourcePath} for {MapName}", resourcePath, mapName);
            return null;
        }
    }

    private static MapCollisionMesh? BuildMeshFromPhysAggregate(string mapName, PhysAggregateData physData)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();

        foreach (var part in physData.Parts)
        {
            if (part.Shape.Meshes.Length == 0)
                continue;

            foreach (var meshDesc in part.Shape.Meshes)
            {
                var mesh = meshDesc.Shape;
                var meshVertices = mesh.GetVertices();
                if (meshVertices.Length == 0)
                    continue;

                var baseIndex = vertices.Count;
                foreach (var vertex in meshVertices)
                    vertices.Add(vertex);

                foreach (var triangle in mesh.GetTriangles())
                {
                    indices.Add(baseIndex + triangle.X);
                    indices.Add(baseIndex + triangle.Y);
                    indices.Add(baseIndex + triangle.Z);
                }
            }
        }

        if (vertices.Count == 0)
            return null;

        return new MapCollisionMesh
        {
            MapName = mapName,
            Vertices = vertices.ToArray(),
            Indices = indices.ToArray()
        };
    }

    private static bool TryLoadCache(string cachePath, string mapName, out MapCollisionMesh? mesh)
    {
        mesh = null;
        if (!File.Exists(cachePath))
            return false;

        try
        {
            using var stream = File.OpenRead(cachePath);
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "MAP1")
                return false;

            var vertexCount = reader.ReadInt32();
            var indexCount = reader.ReadInt32();
            var vertices = new Vector3[vertexCount];
            for (var i = 0; i < vertexCount; i++)
            {
                vertices[i] = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
            }

            var indices = new int[indexCount];
            for (var i = 0; i < indexCount; i++)
                indices[i] = reader.ReadInt32();

            mesh = new MapCollisionMesh
            {
                MapName = mapName,
                Vertices = vertices,
                Indices = indices
            };
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void TrySaveCache(string cachePath, MapCollisionMesh mesh)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            using var stream = File.Create(cachePath);
            using var writer = new BinaryWriter(stream);
            writer.Write(Encoding.ASCII.GetBytes("MAP1"));
            writer.Write(mesh.Vertices.Length);
            writer.Write(mesh.Indices.Length);
            foreach (var vertex in mesh.Vertices)
            {
                writer.Write(vertex.X);
                writer.Write(vertex.Y);
                writer.Write(vertex.Z);
            }

            foreach (var index in mesh.Indices)
                writer.Write(index);
        }
        catch
        {
            // Cache is best-effort.
        }
    }
}
