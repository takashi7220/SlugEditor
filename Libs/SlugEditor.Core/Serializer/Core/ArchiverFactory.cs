using SlugEditor.Core.Assembly;
using SlugEditor.Core.Service;

namespace SlugEditor.Core.Serializer;

[Service(ServiceType.Singleton, ServiceRegisterType.Manual)]
public sealed class ArchiverFactory : IService
{
    private const string JsonBackendName = "Json";

    private readonly Dictionary<Type, CustomSerializer> _customSerializerMap;
    private readonly Dictionary<Type, IDataTemplateSerializer> _dataTemplateMap;
    private readonly Dictionary<string, Func<IBackend>> _backendFactories = new(StringComparer.OrdinalIgnoreCase);

    public ArchiverFactory(AssemblyService assemblyService)
    {
        ArgumentNullException.ThrowIfNull(assemblyService);

        _customSerializerMap = new Dictionary<Type, CustomSerializer>();
        _dataTemplateMap = new Dictionary<Type, IDataTemplateSerializer>();

        RegisterBackend(JsonBackendName, static () => new JsonBackend());
        CollectSerializers(assemblyService);
    }

    public IReadOnlyDictionary<Type, CustomSerializer> CustomSerializers => _customSerializerMap;
    public IReadOnlyDictionary<Type, IDataTemplateSerializer> DataTemplates => _dataTemplateMap;

    public void RegisterBackend(string backendName, Func<IBackend> backendFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backendName);
        ArgumentNullException.ThrowIfNull(backendFactory);
        _backendFactories[backendName] = backendFactory;
    }

    public IArchiver Generate(string backendName = JsonBackendName)
    {
        if (!_backendFactories.TryGetValue(backendName, out var factory))
        {
            throw new KeyNotFoundException($"Backend '{backendName}' is not registered.");
        }

        return Generate(factory());
    }

    public IArchiver Generate(IBackend backend)
    {
        ArgumentNullException.ThrowIfNull(backend);

        return new DefaultArchiver(
            backend,
            customSerializers: _customSerializerMap.Values,
            dataTemplates: _dataTemplateMap.Values);
    }

    private void CollectSerializers(AssemblyService assemblyService)
    {
        var assemblies = assemblyService
            .EnumerateAssemblies()
            .Append(typeof(ArchiverFactory).Assembly)
            .Distinct();

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                if (typeof(CustomSerializer).IsAssignableFrom(type))
                {
                    if (TryCreateInstance(type) is CustomSerializer custom)
                    {
                        _customSerializerMap[custom.TargetType] = custom;
                    }
                    continue;
                }

                if (typeof(IDataTemplateSerializer).IsAssignableFrom(type))
                {
                    if (TryCreateInstance(type) is IDataTemplateSerializer template)
                    {
                        _dataTemplateMap[template.TargetType] = template;
                    }
                }
            }
        }
    }

    private static object? TryCreateInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }
}
