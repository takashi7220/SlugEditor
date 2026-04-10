using SlugEditor.Core.Serializer.Archivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
