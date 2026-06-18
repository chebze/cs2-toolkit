using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Cs2Toolkit.Maps;

public sealed class Cs2MapsLocation
{
    public required string MapsDirectory { get; init; }
    public required string DiscoverySource { get; init; }
}

public static class Cs2InstallLocator
{
    private const string Cs2FolderName = "Counter-Strike Global Offensive";
    private static readonly string[] MapPrefixes = ["de_", "cs_", "ar_"];
    private static readonly Regex ChunkVpkPattern = new(@"_\d{3}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly string[] MapsRelativePaths =
    [
        Path.Combine("game", "csgo", "maps"),
        Path.Combine("csgo", "maps")
    ];

    public static Cs2MapsLocation? LocateMapsDirectory()
    {
        if (TryFindMapsFromRunningProcess(out var fromProcess))
            return fromProcess;

        if (TryFindMapsFromSecondarySteamLibraries(out var fromSecondary))
            return fromSecondary;

        if (TryFindMapsFromSteamInstall(out var fromSteam))
            return fromSteam;

        return null;
    }

    public static string? FindMapsDirectory() => LocateMapsDirectory()?.MapsDirectory;

    public static IEnumerable<string> EnumerateMapVpks(string mapsDirectory)
    {
        if (!Directory.Exists(mapsDirectory))
            yield break;

        var mapNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.EnumerateFiles(mapsDirectory, "*.vpk"))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrWhiteSpace(fileName))
                continue;

            if (fileName.EndsWith("_vanity", StringComparison.OrdinalIgnoreCase))
                continue;

            var mapName = fileName;
            if (mapName.EndsWith("_dir", StringComparison.OrdinalIgnoreCase))
                mapName = mapName[..^4];
            else if (IsChunkVpk(mapName))
                continue;

            if (!IsPlayableMapName(mapName))
                continue;

            mapNames.Add(mapName);
        }

        foreach (var mapName in mapNames.OrderBy(static name => name, StringComparer.OrdinalIgnoreCase))
        {
            var vpkPath = ResolveMapVpkPath(mapsDirectory, mapName);
            if (vpkPath is not null)
                yield return vpkPath;
        }
    }

    public static string? ResolveMapVpkPath(string mapsDirectory, string mapName)
    {
        var dirVpk = Path.Combine(mapsDirectory, $"{mapName}_dir.vpk");
        if (File.Exists(dirVpk))
            return dirVpk;

        var baseVpk = Path.Combine(mapsDirectory, $"{mapName}.vpk");
        if (File.Exists(baseVpk))
            return baseVpk;

        return null;
    }

    private static bool IsPlayableMapName(string mapName) =>
        MapPrefixes.Any(prefix => mapName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static bool IsChunkVpk(string fileName) => ChunkVpkPattern.IsMatch(fileName);

    private static bool TryFindMapsFromRunningProcess(out Cs2MapsLocation? location)
    {
        location = null;

        foreach (var process in Process.GetProcessesByName("cs2"))
        {
            using (process)
            {
                string? executablePath;
                try
                {
                    executablePath = process.MainModule?.FileName;
                }
                catch (Exception)
                {
                    continue;
                }

                var mapsDirectory = TryResolveMapsNearExecutable(executablePath);
                if (mapsDirectory is null)
                    continue;

                location = new Cs2MapsLocation
                {
                    MapsDirectory = mapsDirectory,
                    DiscoverySource = $"running cs2 process (pid {process.Id})"
                };
                return true;
            }
        }

        return false;
    }

    private static bool TryFindMapsFromSecondarySteamLibraries(out Cs2MapsLocation? location)
    {
        location = null;

        foreach (var libraryRoot in GetSecondarySteamLibraryRoots())
        {
            var mapsDirectory = TryResolveMapsFromLibraryRoot(libraryRoot);
            if (mapsDirectory is null)
                continue;

            location = new Cs2MapsLocation
            {
                MapsDirectory = mapsDirectory,
                DiscoverySource = $"secondary SteamLibrary ({libraryRoot})"
            };
            return true;
        }

        return false;
    }

    private static bool TryFindMapsFromSteamInstall(out Cs2MapsLocation? location)
    {
        location = null;

        foreach (var libraryRoot in GetRegistrySteamLibraryRoots())
        {
            var mapsDirectory = TryResolveMapsFromLibraryRoot(libraryRoot);
            if (mapsDirectory is null)
                continue;

            location = new Cs2MapsLocation
            {
                MapsDirectory = mapsDirectory,
                DiscoverySource = $"Steam install ({libraryRoot})"
            };
            return true;
        }

        return false;
    }

    private static IEnumerable<string> GetSecondarySteamLibraryRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady || drive.DriveType is not (DriveType.Fixed or DriveType.Removable))
                continue;

            var root = drive.RootDirectory.FullName;
            if (IsPrimarySystemDrive(root))
                continue;

            var steamLibraryRoot = Path.Combine(root, "SteamLibrary");
            if (!Directory.Exists(steamLibraryRoot))
                continue;

            roots.Add(Path.Combine(steamLibraryRoot, "steamapps"));
        }

        return roots;
    }

    private static IEnumerable<string> GetRegistrySteamLibraryRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var steamPath in GetSteamInstallPaths())
        {
            roots.Add(Path.Combine(steamPath, "steamapps"));

            var libraryFolders = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFolders))
                continue;

            foreach (Match match in Regex.Matches(File.ReadAllText(libraryFolders), "\"path\"\\s+\"([^\"]+)\""))
            {
                if (match.Groups.Count > 1)
                    roots.Add(Path.Combine(NormalizeVdfPath(match.Groups[1].Value), "steamapps"));
            }
        }

        return roots;
    }

    private static string? TryResolveMapsFromLibraryRoot(string libraryRoot)
    {
        foreach (var mapsRelativePath in MapsRelativePaths)
        {
            var mapsDirectory = Path.Combine(libraryRoot, "common", Cs2FolderName, mapsRelativePath);
            if (Directory.Exists(mapsDirectory))
                return mapsDirectory;
        }

        return null;
    }

    private static string? TryResolveMapsNearExecutable(string? executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            return null;

        var current = Path.GetDirectoryName(executablePath);
        for (var depth = 0; depth < 8 && !string.IsNullOrWhiteSpace(current); depth++)
        {
            foreach (var mapsRelativePath in MapsRelativePaths)
            {
                var mapsDirectory = Path.Combine(current, mapsRelativePath);
                if (Directory.Exists(mapsDirectory))
                    return mapsDirectory;
            }

            current = Path.GetDirectoryName(current);
        }

        return null;
    }

    private static IEnumerable<string> GetSteamInstallPaths()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var keyPath in new[]
                 {
                     @"SOFTWARE\WOW6432Node\Valve\Steam",
                     @"SOFTWARE\Valve\Steam"
                 })
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath)
                ?? Registry.CurrentUser.OpenSubKey(keyPath);
            var installPath = key?.GetValue("SteamPath") as string;
            if (!string.IsNullOrWhiteSpace(installPath))
                paths.Add(NormalizeVdfPath(installPath));
        }

        foreach (var envPath in new[] { Environment.GetEnvironmentVariable("ProgramFiles(X86)"), Environment.GetEnvironmentVariable("ProgramFiles") })
        {
            if (string.IsNullOrWhiteSpace(envPath))
                continue;

            var candidate = Path.Combine(envPath, "Steam");
            if (Directory.Exists(candidate))
                paths.Add(candidate);
        }

        return paths;
    }

    private static bool IsPrimarySystemDrive(string rootPath)
    {
        var systemDrive = Environment.GetEnvironmentVariable("SystemDrive");
        if (string.IsNullOrWhiteSpace(systemDrive))
            return rootPath.StartsWith(@"C:\", StringComparison.OrdinalIgnoreCase);

        var normalized = systemDrive.TrimEnd('\\') + @"\";
        return rootPath.StartsWith(normalized, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeVdfPath(string path) =>
        path.Replace(@"\\", @"\");
}
