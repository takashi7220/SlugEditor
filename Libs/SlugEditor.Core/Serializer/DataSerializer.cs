using SlugEditor.Core.Serializer.Archivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.Core.Serializer
{
    public interface IDataSerializer
    {
        public void Serialize(IArchiver ar, int version);
        public object? Deserialize(IArchiver ar, int version);

        public Type? Type { get; set; }
    }

    public class DataSerializer : IDataSerializer
    {
        public DataSerializer()
        {
            Type = null;
        }

        public DataSerializer(Type type) 
        {
            Type = type;
        }

        public virtual void Serialize(IArchiver ar, int version) 
        {
        
        }

        public virtual object? Deserialize(IArchiver ar, int version) 
        {
            return null;
        }

        public Type? Type { get; set; }

    }
}
