namespace SlugEditor.Core.Serializer;

public interface IArchiver
{
    IBackend Backend { get; }

    ArchiveNode Archive<T>(T value, string? name = null);
    T Unarchive<T>(ArchiveNode node);
    object? Unarchive(Type type, ArchiveNode node);
}
