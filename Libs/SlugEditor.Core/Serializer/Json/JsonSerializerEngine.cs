namespace SlugEditor.Core.Serializer;

public sealed class JsonSerializerEngine : SerializerEngine
{
    public JsonSerializerEngine(
        IEnumerable<CustomSerializer>? customSerializers = null,
        IEnumerable<IDataTemplateSerializer>? dataTemplates = null)
        : base(new JsonBackend(), customSerializers, dataTemplates)
    {
    }
}
