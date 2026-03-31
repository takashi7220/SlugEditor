using System.Text.Json;

namespace SlugEditor.Core.Serializer;

public sealed class JsonBackend : IBackend
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public string Name => "Json";

    public string Write(ArchiveNode root)
    {
        return JsonSerializer.Serialize(root, Options);
    }

    public ArchiveNode Read(string raw)
    {
        var node = JsonSerializer.Deserialize<ArchiveNode>(raw, Options)
            ?? throw new InvalidOperationException("Failed to deserialize archive node from json.");

        NormalizeNode(node);
        return node;
    }

    private static void NormalizeNode(ArchiveNode node)
    {
        if (node.Value is JsonElement json)
        {
            node.Value = NormalizeJsonValue(json);
        }

        foreach (var child in node.Children)
        {
            NormalizeNode(child);
        }

        foreach (var key in node.Metadata.Keys.ToArray())
        {
            if (node.Metadata[key] is JsonElement metadataJson)
            {
                node.Metadata[key] = NormalizeJsonValue(metadataJson);
            }
        }
    }

    private static object? NormalizeJsonValue(JsonElement json)
    {
        return json.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.False => false,
            JsonValueKind.True => true,
            JsonValueKind.String => json.GetString(),
            JsonValueKind.Number => json.TryGetInt64(out var l) ? l : json.GetDouble(),
            JsonValueKind.Array => json.EnumerateArray().Select(NormalizeJsonValue).ToList(),
            JsonValueKind.Object => json.EnumerateObject().ToDictionary(x => x.Name, x => NormalizeJsonValue(x.Value)),
            _ => json.GetRawText()
        };
    }
}
