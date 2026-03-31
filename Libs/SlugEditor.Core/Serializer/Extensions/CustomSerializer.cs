namespace SlugEditor.Core.Serializer;

public abstract class CustomSerializer
{
    public abstract Type TargetType { get; }

    public abstract ArchiveNode Serialize(IArchiver archiver, object value, string? name = null);
    public abstract object? Deserialize(IArchiver archiver, ArchiveNode node);
}
