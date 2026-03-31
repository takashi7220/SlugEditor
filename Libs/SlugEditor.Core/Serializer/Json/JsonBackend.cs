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
        return JsonSerializer.Deserialize<ArchiveNode>(raw, Options)
            ?? throw new InvalidOperationException("Failed to deserialize archive node from json.");
    }
}
