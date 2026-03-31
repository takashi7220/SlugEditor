namespace SlugEditor.Core.Serializer;

public interface IDataTemplateSerializer
{
    Type TargetType { get; }

    ArchiveNode Serialize(IArchiver archiver, object value, string? name = null);
    object? Deserialize(IArchiver archiver, ArchiveNode node);
}

public abstract class DataTemplateSerializer<T> : IDataTemplateSerializer
{
    public Type TargetType => typeof(T);
    public virtual string? TemplateName => typeof(T).FullName;

    public abstract ArchiveNode Serialize(IArchiver archiver, T value, string? name = null);
    public abstract T? Deserialize(IArchiver archiver, ArchiveNode node);

    ArchiveNode IDataTemplateSerializer.Serialize(IArchiver archiver, object value, string? name)
    {
        return Serialize(archiver, (T)value, name);
    }

    object? IDataTemplateSerializer.Deserialize(IArchiver archiver, ArchiveNode node)
    {
        return Deserialize(archiver, node);
    }
}
