using SlugEditor.Core.Serializer.Archivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SlugEditor.Core.Serializer.Backend
{
    public interface IDeserializerBackend
    {
        public void BeginStructure(string propertyName, out string propertyType, out int propertyVersion);
        public void EndStructure();
        public void BeginArray(string propertyName, out int count);
        public void EndArray();
        public void BeginArrayItem(int index);
        public void EndArrayItem();
        public bool TryGetValue<T>(string propertyName, ref T value);
        public bool TryGetValue(string propertyName, Type propertyType, ref object? value);
        public IEnumerable<string> GetPropertyNames();
        public void GetStructInfo(out string propertyType, out int propertyVersion);
    }
}
