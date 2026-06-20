using System.Text.Json;
using System.Text.Json.Serialization;

namespace CS2Toolkit.API.Json;

public static class ToolkitJsonSerializerOptions
{
    public static JsonSerializerOptions Web { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
