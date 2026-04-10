using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.Core.Serializer
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializeVersion : Attribute
    {
        public SerializeVersion(int version) 
        {
            Version = version;
        }

        public int Version { get; set; } = 0;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SerializeIgnore : Attribute 
    {
        public SerializeIgnore() 
        {        
        }

        public static bool GetIgnore(Type type)
        {
            var serializeIgnore = type.GetCustomAttribute<SerializeIgnore>();
            return serializeIgnore != null;
        }

        public static bool GetIgnore(Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName);
            if (property != null) 
            {
                var serializeIgnore = property.GetCustomAttribute<SerializeIgnore>();
                return serializeIgnore != null;
            }
            return false;
        }
    }
}
