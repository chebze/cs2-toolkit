using System.Text.Json;
using System.Text.Json.Serialization;

namespace CS2Toolkit.Configuration;

internal static class ConfigurationJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
