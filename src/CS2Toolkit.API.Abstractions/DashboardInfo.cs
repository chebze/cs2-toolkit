namespace CS2Toolkit.API.Abstractions;

public sealed record ActiveProfileSummary(string Id, string Name, string? SwitchHotkey);

public sealed record DashboardInfo(
    ActiveProfileSummary ActiveProfile,
    string DefaultProfileId,
    IReadOnlyList<string> AccessUrls,
    int WebPort,
    string RadarUrl);
