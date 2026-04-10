using SlugEditor.Core.Serializer.Backend;
using System.Collections;

namespace SlugEditor.Core.Serializer.Archivers
{
    public class ArchiverDeserializer
    {
        public ArchiverDeserializer(IDeserializerBackend backEnd) 
        {
            m_backEnd = backEnd;
        }

        public object? Deserialize(IArchiver ar, Type propertyType, string propertyName)
        {
            if (!SerializeIgnore.GetIgnore(propertyType))
            {
                // DataSerialize
                if (ar.Options?.DataSerializerTable?.TryGetValue(propertyType, out IDataSerializer? dataSerializer) ?? false)
                {
                    m_backEnd.BeginStructure(propertyName, out var structureType, out var structureVersion);
                    var obj = dataSerializer?.Deserialize(ar, structureVersion);
                    m_backEnd.EndStructure();
                    return obj;
                }
                else if (typeof(ICustomSerializer).IsAssignableFrom(propertyType))
                {
                    var obj = Activator.CreateInstance(propertyType);
                    if (obj is ICustomSerializer custom)
                    {
                        if (string.IsNullOrEmpty(propertyName))
                        {
                            m_backEnd.GetStructInfo(out var structureType, out var structureVersion);
                            custom.Deserialize(ar, structureVersion);
                        }
                        else
                        {
                            m_backEnd.BeginStructure(propertyName, out var structureType, out var structureVersion);
                            custom.Deserialize(ar, structureVersion);
                            m_backEnd.EndStructure();
                        }
                    }
                    return obj;
                }
                else if(propertyType == typeof(string))
                {
                    string str = string.Empty;
                    if (m_backEnd.TryGetValue(propertyName, ref str))
                    {
                        return str;
                    }
                    return str;
                }
                // Dictionary
                else if (typeof(IDictionary).IsAssignableFrom(propertyType))
                {
                    var obj = Activator.CreateInstance(propertyType);
                    m_backEnd.BeginStructure(propertyName, out var structureType, out var structureVersion);
                    var valueType = propertyType.GetGenericArguments()?[1] ?? null;
                    if (valueType?.IsClass ?? false && valueType != typeof(string))
                    {
                        valueType = GetPropertyType(ar, valueType, structureType);
                    }

                    if (valueType != null && obj != null)
                    {
                        var dict = (IDictionary)obj;
                        foreach (var key in m_backEnd.GetPropertyNames())
                        {
                            var item = Deserialize(ar, valueType, key);
                            dict.Add(key, item);
                        }
                    }
                    m_backEnd.EndStructure();
                    return obj;
                }
                // IEnumerable
                else if (typeof(IList).IsAssignableFrom(propertyType) || propertyType.IsArray)
                {
                    m_backEnd.BeginArray(propertyName, out var count);
                    var obj = Activator.CreateInstance(propertyType, count);
                    if (obj is IList list)
                    {
                        var valueType = propertyType.GetElementType();
                        if (!propertyType.IsArray && propertyType.IsGenericType)
                        {
                            valueType = propertyType.GetGenericArguments()[0];
                        }

                        if (valueType != null)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                m_backEnd.BeginArrayItem(i);
                                if (valueType.IsClass && valueType != typeof(string))
                                {
                                    m_backEnd.GetStructInfo(out var structureType, out var structureVersion);
                                    valueType = GetPropertyType(ar, valueType, structureType);
                                }

                                list[i] = Deserialize(ar, valueType, string.Empty);
                                m_backEnd.EndArrayItem();
                            }
                        }
                    }
                    m_backEnd.EndArray();
                    return obj;
                }
                // Class/Struct
                else if (propertyType.IsClass)
                {
                    object? obj = null;
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        m_backEnd.GetStructInfo(out var structureType, out var structureVersion);
                        propertyType = GetPropertyType(ar, propertyType, structureType);
                        obj = Activator.CreateInstance(propertyType);

                        foreach (var key in m_backEnd.GetPropertyNames())
                        {
                            var property = propertyType.GetProperty(key);
                            if (obj != null && property != null)
                            {
                                var item = Deserialize(ar, property.PropertyType, key);
                                property?.SetValue(obj, item);
                            }
                        }
                    }
                    else
                    {
                        m_backEnd.BeginStructure(propertyName, out var structureType, out var structureVersion);
                        propertyType = GetPropertyType(ar, propertyType, structureType);
                        obj = Activator.CreateInstance(propertyType);
                        foreach (var key in m_backEnd.GetPropertyNames())
                        {
                            var property = propertyType.GetProperty(key);
                            if (obj != null && property != null)
                            {
                                var item = Deserialize(ar, property.PropertyType, key);
                                property?.SetValue(obj, item);
                            }
                        }
                        m_backEnd.EndStructure();
                    }
                    return obj;
                }
                else
                {
                    object? obj = Activator.CreateInstance(propertyType);
                    if (obj != null)
                    {
                        if (m_backEnd.TryGetValue(propertyName, propertyType, ref obj))
                        {
                            return obj;
                        }
                    }
                    return obj;
                }
            }
            return null;
        }

        private Type GetPropertyType(IArchiver ar, Type propertyType, string propertyTypeName) 
        {
            if (ar.Options?.SerializeTypeTable?.TryGetValue(propertyTypeName, out Type? tmpType) ?? false)
            {
                if (tmpType != null) 
                {
                    return tmpType;
                }
            }
            return propertyType;
        }

        private IDeserializerBackend m_backEnd;
    }
}
