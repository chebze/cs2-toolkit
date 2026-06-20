using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Cs2Toolkit.Configuration;

public static class AppSettingsWriter
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static void SaveToolkitSection(string filePath, ToolkitOptions toolkit)
    {
        JsonObject root;
        if (File.Exists(filePath))
        {
            root = JsonNode.Parse(File.ReadAllText(filePath)) as JsonObject
                ?? throw new InvalidOperationException($"Failed to parse settings file: {filePath}");
        }
        else
        {
            root = new JsonObject();
        }

        root[ToolkitOptions.SectionName] = JsonSerializer.SerializeToNode(toolkit, WriteOptions)
            ?? throw new InvalidOperationException("Failed to serialize Toolkit settings.");

        var content = root.ToJsonString(WriteOptions);
        var tempPath = filePath + ".tmp";
        File.WriteAllText(tempPath, content);
        File.Move(tempPath, filePath, overwrite: true);
    }
}
