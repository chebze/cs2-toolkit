using System.Text.Json.Serialization;

namespace Cs2Toolkit.Tunnel;

public sealed class LocalTunnelRegistration
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "";

    [JsonPropertyName("port")]
    public int Port { get; init; }

    [JsonPropertyName("max_conn_count")]
    public int MaxConnectionCount { get; init; } = 1;

    [JsonPropertyName("url")]
    public string Url { get; init; } = "";

    [JsonPropertyName("ip")]
    public string? Ip { get; init; }

    [JsonPropertyName("cached_url")]
    public string? CachedUrl { get; init; }
}
