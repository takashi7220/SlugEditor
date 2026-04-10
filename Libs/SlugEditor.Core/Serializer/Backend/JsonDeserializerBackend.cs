using SlugEditor.Core.Serializer.Archivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SlugEditor.Core.Serializer.Backend
{
    public sealed class JsonDeserializerBackend : IDeserializerBackend
    {
        public JsonDeserializerBackend(byte[] datas)
        {
            string json = Encoding.UTF8.GetString(datas);
            m_root = JsonNode.Parse(json)!.AsObject();

            if (m_root == null) 
            {
                throw new InvalidOperationException("Can't Deserialize Data");
            }

            m_currentRoot = m_root;
            m_parentNodes = new Stack<JsonNode>();
        }

        public void BeginStructure(string propertyName, out string propertyType, out int propertyVersion) 
        {
            var obj = m_currentRoot.AsObject();
            if (obj?.TryGetPropertyValue(propertyName, out var node) ?? false)
            {
                if (node != null)
                {
                    m_parentNodes.Push(m_currentRoot);
                    m_currentRoot = node;

                    var tmp = m_currentRoot.AsObject();                    
                    propertyType = tmp?[SerializerDefine.TypeName]?.GetValue<string>() ?? string.Empty;
                    propertyVersion = tmp?[SerializerDefine.VersionName]?.GetValue<int>() ?? 0;
                    return;
                }
            }

            throw new InvalidOperationException($"Can't Deserialize Property : {propertyName}");
        }

        public void EndStructure() 
        {
            var node = m_parentNodes.Pop();
            if (node != null)
            {
                m_currentRoot = node;
                return;
            }

            throw new InvalidOperationException($"Failed Deserialize ReadEndObject");
        }

        public void BeginArray(string propertyName, out int count)
        {
            count = 0;
            var obj = m_currentRoot.AsObject();
            if (obj?.TryGetPropertyValue(propertyName, out var node) ?? false)
            {
                if (node != null)
                {
                    count = node.AsArray().Count;
                    m_parentNodes.Push(m_currentRoot);
                    m_currentRoot = node;
                    return;
                }
            }

            throw new InvalidOperationException($"Can't Deserialize Property : {propertyName}");
        }
        public void EndArray()
        {
            var node = m_parentNodes.Pop();
            if (node != null)
            {
                m_currentRoot = node;
                return;
            }
            throw new InvalidOperationException($"Failed Deserialize ReadEndArray");
        }

        public void BeginArrayItem(int index) 
        {
            var item = m_currentRoot?.AsArray()[index];
            if (item != null)
            {
                m_parentNodes.Push(m_currentRoot);
                m_currentRoot = item;
                return;
            }
            throw new InvalidOperationException($"Can't Deserialize Property Items[{index}]");
        }

        public void EndArrayItem()
        {
            var node = m_parentNodes.Pop();
            if (node != null)
            {
                m_currentRoot = node;
                return;
            }
            throw new InvalidOperationException($"Failed Deserialize ReadEndArray");
        }

        public bool TryGetValue<T>(string propertyName, ref T value) 
        {
            var obj = m_currentRoot as JsonObject;
            if (obj != null)
            {
                if (obj[propertyName] is JsonValue v)
                {
                    T? valueObj = v.GetValue<T>();
                    if (valueObj != null)
                    {
                        value = valueObj;
                        return true;
                    }
                }
            }
            else if(typeof(T) == typeof(string) || !typeof(T).IsClass)
            {
                value = m_currentRoot.GetValue<T>();
                return true;
            }
            return false;
        }

        public bool TryGetValue(string propertyName, Type propertyType, ref object? value) 
        {
            if (propertyType == typeof(int)) 
            {
                int tmpValue = 0;
                if (TryGetValue(propertyName, ref tmpValue)) 
                {
                    value = tmpValue;
                    return true;
                }
            }
            else if (propertyType == typeof(double))
            {
                double tmpValue = 0;
                if (TryGetValue(propertyName, ref tmpValue))
                {
                    value = tmpValue;
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<string> GetPropertyNames() 
        {
            var names = new List<string>();
            var obj = m_currentRoot.AsObject();
            if (obj != null) 
            {
                foreach (var child in obj) 
                {
                    names.Add(child.Key);
                }
            }
            return names;
        }
        public void GetStructInfo(out string propertyType, out int propertyVersion) 
        {
            var tmp = m_currentRoot.AsObject();
            propertyType = tmp?[SerializerDefine.TypeName]?.GetValue<string>() ?? string.Empty;
            propertyVersion = tmp?[SerializerDefine.VersionName]?.GetValue<int>() ?? 0;
        }

        private Stack<JsonNode> m_parentNodes;
        private JsonNode m_currentRoot;
        private JsonNode m_root;
    }
}
