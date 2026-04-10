using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SlugEditor.Core.Serializer;
using SlugEditor.Core.Serializer.Archivers;
using SlugEditor.Core.Serializer.Backend;

namespace SlugEditor.Core.Serializer.BackEnd
{
    public class JsonSerializerBackend : ISerializerBackend
    {
        public JsonSerializerBackend(Stream stream) 
        {
            m_stream = stream;
            m_writer = new Utf8JsonWriter(m_stream, new JsonWriterOptions { Indented = true });
        }

        public void BeginArray(SerializeState state)
        {
            if (!string.IsNullOrEmpty(state.PropertyName))
            {
                m_writer.WriteStartArray(state.PropertyName);
            }
            else
            {
                m_writer.WriteStartArray();
            }
        }

        public void EndArray()
        {
            m_writer.WriteEndArray();
        }

        public void BeginStructure(SerializeState state, bool writeStructMeta = true)
        {
            if (!string.IsNullOrEmpty(state.PropertyName))
            {
                m_writer.WriteStartObject(state.PropertyName);
            }
            else
            {
                m_writer.WriteStartObject();
            }

            if (writeStructMeta)
            {
                WriteType(state);
                WriteVersion(state);
            }
        }

        public void EndStructure()
        {
            m_writer.WriteEndObject();
        }

        public void WriteType(SerializeState state)
        {
            if (state.PropertyType != null)
            {
                m_writer.WritePropertyName(SerializerDefine.TypeName);
                m_writer.WriteStringValue(state.PropertyType.FullName);
            }
        }

        public void WriteVersion(SerializeState state) 
        {
            m_writer.WritePropertyName(SerializerDefine.VersionName);
            m_writer.WriteNumberValue(state.Version);
        }

        public void WriteProperty(SerializeState state)
        {
            if (!string.IsNullOrEmpty(state.PropertyName))
            {
                m_writer.WritePropertyName(state.PropertyName);
            }

            if (state.PropertyValue is bool boolValue)
            {
                m_writer.WriteBooleanValue(boolValue);
            }
            else if (state.PropertyValue is byte byteValue)
            {
                m_writer.WriteNumberValue(byteValue);
            }
            else if (state.PropertyValue is sbyte sbyteValue)
            {
                m_writer.WriteNumberValue(sbyteValue);
            }
            else if (state.PropertyValue is short shortValue)
            {
                m_writer.WriteNumberValue(shortValue);
            }
            else if (state.PropertyValue is ushort ushortValue)
            {
                m_writer.WriteNumberValue(ushortValue);
            }
            else if (state.PropertyValue is int intValue)
            {
                m_writer.WriteNumberValue(intValue);
            }
            else if (state.PropertyValue is uint uintValue)
            {
                m_writer.WriteNumberValue(uintValue);
            }
            else if (state.PropertyValue is float floatValue)
            {
                m_writer.WriteNumberValue(floatValue);
            }
            else if (state.PropertyValue is double doubleValue)
            {
                m_writer.WriteNumberValue(doubleValue);
            }
            else if (state.PropertyValue is string stringValue)
            {
                m_writer.WriteStringValue(stringValue);
            }
            else if ((state.PropertyType?.IsEnum ?? false) && state.PropertyValue != null)
            {
                m_writer.WriteNumberValue((int)state.PropertyValue);
            }
            else if (state.PropertyValue == null)
            {
                m_writer.WriteNullValue();
            }
        }

        public void Flush() 
        {
            m_writer.Flush();
        }

        private Utf8JsonWriter m_writer;
        private readonly Stream m_stream;
    }
}
