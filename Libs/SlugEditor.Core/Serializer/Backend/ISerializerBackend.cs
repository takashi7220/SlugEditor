using SlugEditor.Core.Serializer.Archivers;

namespace SlugEditor.Core.Serializer.Backend
{
    public interface ISerializerBackend
    {
        public void BeginArray(SerializeState state);

        public void EndArray();

        public void BeginStructure(SerializeState state, bool writeStructMeta = true);

        public void EndStructure();

        public void WriteType(SerializeState state);

        public void WriteProperty(SerializeState state);

        public void Flush();
    }
}
