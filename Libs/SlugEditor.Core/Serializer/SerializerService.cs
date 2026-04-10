using SlugEditor.Core.Assembly;
using SlugEditor.Core.Serializer.Archivers;
using SlugEditor.Core.Serializer.Backend;
using SlugEditor.Core.Serializer.BackEnd;
using SlugEditor.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.Core.Serializer
{
    public interface ICustomSerializer
    {
        void Serialize(IArchiver ar, int version);

        void Deserialize(IArchiver ar, int version);
    }

    public enum SerializerType 
    {
        Json,
    }

    public class Options
    {
        public Dictionary<Type, IDataSerializer>? DataSerializerTable { get; set; } = null;
        public Dictionary<string, Type>? SerializeTypeTable { get; set; } = null;
        public SerializerType SerializerType = SerializerType.Json;
    };

    [ServiceAttribute(ServiceType.Singleton)]
    public class SerializerService : IService
    {
        public SerializerService(AssemblyService assemblyService)
        {
            Initialize(assemblyService);
        }

        public void Serialize<T>(T target, Stream stream, Options? options = null) where T : notnull
        {
            Serialize(target, typeof(T), stream, options);
        }

        public T Deserialize<T>(byte[] target, Options? options = null) where T : notnull
        {
            var obj = Deserialize(target, typeof(T), options);
            return (T)obj;
        }

        public void Serialize(object target, Type targetType, Stream stream, Options? options = null)
        {
            if (options == null)
            {
                options = MakeOption();
            }
            Archivers.IArchiver ar = CreateArchiver(options.SerializerType, stream);
            Serialize(target, targetType, ar, options);
        }

        public object? Deserialize(byte[] target, Type targetType, Options? options = null)
        {
            if (options == null)
            {
                options = MakeOption();
            }
            Archivers.IArchiver ar = CreateArchiver(options.SerializerType, target);
            return Deserialize(targetType, ar, options);
        }

        public void Serialize(object target, Type targetType, IArchiver archiver, Options? options = null)
        {
            if (options == null)
            {
                options = MakeOption();
            }
            archiver.Options = options;
            archiver.Serialize("", target, targetType);
            archiver.FlushSerialize();
        }

        public object? Deserialize(Type targetType, IArchiver archiver, Options? options = null)
        {
            if (options == null)
            {
                options = MakeOption();
            }
            archiver.Options = options;
            return archiver.Deserialize("",  targetType);
        }

        private Options MakeOption() 
        {
            var options = new Options();

            if (options.DataSerializerTable == null)
            {
                options.DataSerializerTable = m_defaultSerializerTable;
            }

            if (options.SerializeTypeTable == null)
            {
                options.SerializeTypeTable = m_defaultSerializeTypeTable;
            }
            return options;
        }

        private static IArchiver CreateArchiver(SerializerType type, Stream stream)
        {
            if (type == SerializerType.Json)
            {
                return new Archivers.Archiver(new JsonSerializerBackend(stream), null);
            }
            return new Archivers.Archiver(new JsonSerializerBackend(stream), null);
        }

        private static IArchiver CreateArchiver(SerializerType type, byte[] target)
        {
            if (type == SerializerType.Json)
            {
                return new Archivers.Archiver(null, new JsonDeserializerBackend(target));
            }
            return new Archivers.Archiver(null, new JsonDeserializerBackend(target));
        }

        private void Initialize(AssemblyService assemblyService)
        {
            foreach (var assembly in assemblyService.EnumerateAssembly())
            {
                var baseType = typeof(IDataSerializer);
                var derivedTypes = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters && baseType.IsAssignableFrom(t));
                foreach (var type in derivedTypes)
                {
                    var serializer = Activator.CreateInstance(type) as IDataSerializer;
                    if (serializer != null && serializer.Type != null)
                    {
                        m_defaultSerializerTable.Add(type, serializer);
                    }
                }

                foreach (var type in assembly.GetTypes()) 
                {
                    if (type != null)
                    {
                        string typeName = type.FullName != null ? type.FullName : type.Name;
                        if (!m_defaultSerializeTypeTable.ContainsKey(typeName))
                        {
                            m_defaultSerializeTypeTable.Add(typeName, type);
                        }
                    }
                }
            }
        }

        private Dictionary<Type, IDataSerializer> m_defaultSerializerTable = new();
        private Dictionary<string, Type> m_defaultSerializeTypeTable = new();
    }
}
