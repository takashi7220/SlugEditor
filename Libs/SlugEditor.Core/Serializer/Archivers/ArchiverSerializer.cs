using SlugEditor.Core.Serializer.Backend;
using System.Collections;
using System.Reflection;

namespace SlugEditor.Core.Serializer.Archivers
{
    public class ArchiverSerializer
    {
        public ArchiverSerializer(ISerializerBackend backEnd) 
        {
            m_backEnd = backEnd;
        }

        public virtual void Serialize(IArchiver ar, SerializeState currentState)
        {
            if ((currentState.PropertyValue != null) && (currentState.PropertyType != null) && !SerializeIgnore.GetIgnore(currentState.PropertyType))
            {

                // DataSerialize
                if (ar.Options?.DataSerializerTable?.TryGetValue(currentState.PropertyType, out IDataSerializer? dataSerializer) ?? false)
                {
                    dataSerializer?.Serialize(ar, GetVersion(currentState.PropertyType));
                }
                else if (currentState.PropertyValue is ICustomSerializer custom)
                {
                    m_backEnd.BeginStructure(currentState);
                    custom.Serialize(ar, GetVersion(currentState.PropertyType));
                    m_backEnd.EndStructure();
                }
                else if (currentState.PropertyValue is string)
                {
                    m_backEnd.WriteProperty(currentState);
                }
                // Dictionary
                else if (currentState.PropertyValue is IDictionary)
                {
                    m_backEnd.BeginStructure(currentState, false);
                    foreach (var state in SerializeState.GenerateDictionaryStates(currentState))
                    {
                        Serialize(ar, state);
                    }
                    m_backEnd.EndStructure();
                }
                // IEnumerable
                else if (currentState.PropertyValue is IEnumerable)
                {

                    m_backEnd.BeginArray(currentState);
                    foreach (var state in SerializeState.GenerateArrayStates(currentState))
                    {
                        Serialize(ar, state);
                    }
                    m_backEnd.EndArray();
                }
                // Class/Struct
                else if (currentState.PropertyType?.IsClass ?? false)
                {
                    m_backEnd.BeginStructure(currentState);
                    foreach (var state in SerializeState.GenerateClassStates(currentState))
                    {
                        string propertyName = (state.PropertyName) != null ? state.PropertyName : string.Empty;
                        if (!SerializeIgnore.GetIgnore(currentState.PropertyType, propertyName))
                        {
                            Serialize(ar, state);
                        }
                    }
                    m_backEnd.EndStructure();
                }
                else
                {
                    m_backEnd.WriteProperty(currentState);
                }
            }
        }

        public int GetVersion(Type? type)
        {
            var serializeVersion = type?.GetCustomAttribute<SerializeVersion>();
            return serializeVersion?.Version ?? 0;
        }

        public void Flush() 
        {
            m_backEnd.Flush();
        }

        private ISerializerBackend m_backEnd;
    }
}
