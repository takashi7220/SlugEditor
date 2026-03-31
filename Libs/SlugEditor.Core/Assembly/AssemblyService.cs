using SlugEditor.Core.Service;
namespace SlugEditor.Core.Assembly;

[Service(ServiceType.Singleton, ServiceRegisterType.Manual)]
public class AssemblyService(System.Reflection.Assembly executeAssembly) : IService
{
    private readonly System.Reflection.Assembly _executeAssembly = executeAssembly;

    public IEnumerable<System.Reflection.Assembly> EnumerateAssemblies() => EnumerateAssemblies(_executeAssembly);

    private static List<System.Reflection.Assembly> EnumerateAssemblies(System.Reflection.Assembly assembly)
    {
        var assemblyNames = assembly.GetReferencedAssemblies();
        return assemblyNames.Select(System.Reflection.Assembly.Load).ToList();
    }
}
