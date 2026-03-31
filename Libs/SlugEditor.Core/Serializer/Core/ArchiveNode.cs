namespace SlugEditor.Core.Serializer;

public sealed class ArchiveNode
{
    public string? Name { get; set; }
    public string? ValueType { get; set; }
    public object? Value { get; set; }
    public List<ArchiveNode> Children { get; set; } = [];
    public Dictionary<string, object?> Metadata { get; set; } = [];
}
