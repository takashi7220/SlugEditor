using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace SlugEditor.Core.Serializer;

public sealed class DefaultArchiver : IArchiver
{
    private readonly Dictionary<Type, CustomSerializer> _customSerializers;
    private readonly Dictionary<Type, IDataTemplateSerializer> _dataTemplates;

    public DefaultArchiver(
        IBackend backend,
        IEnumerable<CustomSerializer>? customSerializers = null,
        IEnumerable<IDataTemplateSerializer>? dataTemplates = null)
    {
        Backend = backend;
        _customSerializers = (customSerializers ?? []).ToDictionary(x => x.TargetType);
        _dataTemplates = (dataTemplates ?? []).ToDictionary(x => x.TargetType);
    }

    public IBackend Backend { get; }

    public ArchiveNode Archive<T>(T value, string? name = null)
    {
        return ArchiveInternal(typeof(T), value, name);
    }

    public T Unarchive<T>(ArchiveNode node)
    {
        return (T?)Unarchive(typeof(T), node) ?? throw new InvalidOperationException("Deserialized value is null.");
    }

    public object? Unarchive(Type type, ArchiveNode node)
    {
        if (_customSerializers.TryGetValue(type, out var custom))
        {
            return custom.Deserialize(this, node);
        }

        if (_dataTemplates.TryGetValue(type, out var template))
        {
            return template.Deserialize(this, node);
        }

        if (node.Value is null && node.Children.Count is 0)
        {
            return null;
        }

        if (IsPrimitiveLike(type))
        {
            return ConvertValue(type, node.Value);
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType() ?? throw new InvalidOperationException("Array element type not found.");
            var array = Array.CreateInstance(elementType, node.Children.Count);
            for (var i = 0; i < node.Children.Count; i++)
            {
                array.SetValue(Unarchive(elementType, node.Children[i]), i);
            }
            return array;
        }

        if (typeof(IList).IsAssignableFrom(type))
        {
            var itemType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
            var list = (IList)(Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Could not create list type {type}."));
            foreach (var child in node.Children)
            {
                list.Add(Unarchive(itemType, child));
            }
            return list;
        }

        var instance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Could not create type {type}.");

        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!prop.CanWrite)
            {
                continue;
            }

            var child = node.Children.FirstOrDefault(x => string.Equals(x.Name, prop.Name, StringComparison.Ordinal));
            if (child is null)
            {
                continue;
            }

            prop.SetValue(instance, Unarchive(prop.PropertyType, child));
        }

        return instance;
    }

    private ArchiveNode ArchiveInternal(Type type, object? value, string? name)
    {
        if (value is null)
        {
            return new ArchiveNode { Name = name, ValueType = type.AssemblyQualifiedName };
        }

        var runtimeType = value.GetType();

        if (_customSerializers.TryGetValue(runtimeType, out var custom))
        {
            return custom.Serialize(this, value, name);
        }

        if (_dataTemplates.TryGetValue(runtimeType, out var template))
        {
            return template.Serialize(this, value, name);
        }

        if (IsPrimitiveLike(runtimeType))
        {
            return new ArchiveNode
            {
                Name = name,
                ValueType = runtimeType.AssemblyQualifiedName,
                Value = value
            };
        }

        if (value is Array arrayValue)
        {
            var children = new List<ArchiveNode>();
            for (var i = 0; i < arrayValue.Length; i++)
            {
                children.Add(ArchiveInternal(runtimeType.GetElementType() ?? typeof(object), arrayValue.GetValue(i), i.ToString()));
            }

            return new ArchiveNode
            {
                Name = name,
                ValueType = runtimeType.AssemblyQualifiedName,
                Children = children
            };
        }

        if (value is IList list)
        {
            var itemType = runtimeType.IsGenericType ? runtimeType.GetGenericArguments()[0] : typeof(object);
            var children = new List<ArchiveNode>();
            for (var i = 0; i < list.Count; i++)
            {
                children.Add(ArchiveInternal(itemType, list[i], i.ToString()));
            }

            return new ArchiveNode
            {
                Name = name,
                ValueType = runtimeType.AssemblyQualifiedName,
                Children = children
            };
        }

        var node = new ArchiveNode
        {
            Name = name,
            ValueType = runtimeType.AssemblyQualifiedName
        };

        var props = runtimeType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.CanRead)
            .ToArray();
        foreach (var prop in props)
        {
            node.Children.Add(ArchiveInternal(prop.PropertyType, prop.GetValue(value), prop.Name));
        }

        return node;
    }

    private static bool IsPrimitiveLike(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(Guid)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan);
    }

    private static object? ConvertValue(Type type, object? rawValue)
    {
        if (rawValue is null)
        {
            return null;
        }

        if (rawValue is JsonElement json)
        {
            return ConvertJsonElement(type, json);
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, rawValue.ToString() ?? string.Empty, ignoreCase: true);
        }

        return Convert.ChangeType(rawValue, type);
    }

    private static object? ConvertJsonElement(Type type, JsonElement json)
    {
        if (type == typeof(string))
        {
            return json.GetString();
        }

        if (type == typeof(int))
        {
            return json.GetInt32();
        }

        if (type == typeof(long))
        {
            return json.GetInt64();
        }

        if (type == typeof(float))
        {
            return json.GetSingle();
        }

        if (type == typeof(double))
        {
            return json.GetDouble();
        }

        if (type == typeof(decimal))
        {
            return json.GetDecimal();
        }

        if (type == typeof(bool))
        {
            return json.GetBoolean();
        }

        if (type == typeof(Guid))
        {
            return json.GetGuid();
        }

        if (type == typeof(DateTime))
        {
            return json.GetDateTime();
        }

        if (type == typeof(DateTimeOffset))
        {
            return json.GetDateTimeOffset();
        }

        if (type == typeof(TimeSpan))
        {
            return TimeSpan.Parse(json.GetString() ?? "00:00:00");
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, json.GetString() ?? string.Empty, ignoreCase: true);
        }

        var raw = json.GetRawText();
        return JsonSerializer.Deserialize(raw, type);
    }
}
