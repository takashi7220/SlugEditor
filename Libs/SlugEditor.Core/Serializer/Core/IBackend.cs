namespace SlugEditor.Core.Serializer;

public interface IBackend
{
    string Name { get; }

    string Write(ArchiveNode root);
    ArchiveNode Read(string raw);
}
