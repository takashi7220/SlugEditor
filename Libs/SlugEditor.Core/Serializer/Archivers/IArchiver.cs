using SlugEditor.Core.Serializer.Backend;
using SlugEditor.Core.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace SlugEditor.Core.Serializer.Archivers
{
    public interface IArchiver
    {
        public void Deserialize<T>(string propertyName, Type propertyType, ref T dst);

        public object? Deserialize(string propertyName, Type propertyType);

        public void Serialize(string propertyName, object propertyValue, Type propertyType);

        public void FlushSerialize();

        public void SerializeBaseClass<BaseType>(object value) where BaseType : class;

        public Options? Options { get; set; }
    }
}
