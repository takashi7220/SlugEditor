namespace SlugEditor.Core.Serializer;

public class SerializerEngine
{
    private readonly DefaultArchiver _archiver;

    public SerializerEngine(
        IBackend backend,
        IEnumerable<CustomSerializer>? customSerializers = null,
        IEnumerable<IDataTemplateSerializer>? dataTemplates = null)
    {
        _archiver = new DefaultArchiver(backend, customSerializers, dataTemplates);
    }

    public string SerializeToString<T>(T value, string? rootName = null)
    {
        var root = _archiver.Archive(value, rootName);
        return _archiver.Backend.Write(root);
    }

    public T DeserializeFromString<T>(string raw)
    {
        var root = _archiver.Backend.Read(raw);
        return _archiver.Unarchive<T>(root);
    }
}
