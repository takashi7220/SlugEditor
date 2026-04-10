using SlugEditor.Core.Serializer.Backend;

namespace SlugEditor.Core.Serializer.Archivers
{
    public class Archiver : IArchiver
    {
        public Archiver(ISerializerBackend? serializer, IDeserializerBackend? deserializer)
        {
            if (serializer != null)
            {
                m_serializer = new ArchiverSerializer(serializer);
            }

            if (deserializer != null)
            {
                m_deserializer = new ArchiverDeserializer(deserializer);
            }
        }

        public virtual void Serialize(string propertyName, object propertyValue, Type propertyType)
        {
            if (m_serializer != null)
            {
                m_serializer.Serialize(this, SerializeState.GenerateSerializeState(propertyName, propertyValue, propertyType));
            }
        }

        public virtual void Deserialize<T>(string propertyName, Type propertyType, ref T dst)
        {
            object? tmp = Deserialize(propertyName, propertyType);
            if (tmp != null)
            {
                dst = (T)tmp;
            }
        }

        public virtual object? Deserialize(string propertyName, Type propertyType)
        {
            object? tmp = null;
            if (m_deserializer != null)
            {
                tmp = m_deserializer.Deserialize(this, propertyType, propertyName);
            }
            return tmp;
        }

        public virtual void SerializeBaseClass<BaseType>(object value) where BaseType : class
        {
            if (m_serializer != null)
            {
                BaseType? target = null;
                if (value.GetType().Equals(typeof(BaseType))
                    || value is BaseType)
                {
                    target = (BaseType)value;
                }

                if (target != null)
                {
                    foreach (var state in SerializeState.GenerateClassStates(SerializeState.GenerateSerializeState("", target, typeof(BaseType))))
                    {
                        m_serializer.Serialize(this, state);
                    }
                }
            }
        }

        public void FlushSerialize()
        {
            m_serializer?.Flush();
        }

        public Options? Options { get; set; }

        protected ArchiverSerializer? m_serializer;

        protected ArchiverDeserializer? m_deserializer;
    }
}
