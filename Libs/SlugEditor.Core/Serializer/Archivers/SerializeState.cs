using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.Core.Serializer.Archivers
{
    public struct SerializeState
    {
        public Type? PropertyType { get; set; }
        public string? PropertyName { get; set; }
        public object? PropertyValue { get; set; }
        public int Version { get; set; }

        public static IEnumerable<SerializeState> GenerateClassStates(SerializeState state)
        {
            var states = new List<SerializeState>();
            var type = state.PropertyType;
            if (type != null)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    states.Add(GenerateSerializeState(property.Name, property.GetValue(state.PropertyValue), property.PropertyType));
                }
            }
            return states;
        }

        public static IEnumerable<SerializeState> GenerateArrayStates(SerializeState state)
        {
            var states = new List<SerializeState>();
            var enumerable = state.PropertyValue as IEnumerable;
            if (enumerable != null)
            {
                foreach (var item in enumerable)
                {
                    states.Add(GenerateSerializeState("", item, item.GetType()));
                }
            }
            return states;
        }
        public static IEnumerable<SerializeState> GenerateDictionaryStates(SerializeState state)
        {
            var states = new List<SerializeState>();
            var dictionary = state.PropertyValue as IDictionary;
            if (dictionary != null)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    states.Add(GenerateSerializeState(entry.Key?.ToString() ?? "", entry.Value ?? null, entry.Value?.GetType() ?? null));
                }
            }
            return states;
        }

        public static SerializeState GenerateSerializeState(string propertyName, object? propertyValue, Type? propertyType)
        {
            var state = new SerializeState();
            state.PropertyName = propertyName;
            state.PropertyValue = propertyValue;
            state.PropertyType = propertyType;
            state.Version = GetVersion(propertyType);
            return state;
        }

        public static int GetVersion(Type? type)
        {
            var serializeVersion = type?.GetCustomAttribute<SerializeVersion>();
            return serializeVersion?.Version ?? 0;
        }
    }

}
