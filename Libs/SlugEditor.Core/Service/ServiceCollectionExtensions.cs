using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SlugEditor.Core.Assembly;

namespace SlugEditor.Core.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, AssemblyService assemblyService)
    {
        foreach (var assembly in assemblyService.EnumerateAssemblies())
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                if (ReferenceEquals(type, typeof(AssemblyService)))
                {
                    continue;
                }

                var interfaces = type.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!ReferenceEquals(@interface, typeof(IService)))
                    {
                        continue;
                    }
                    var serviceAttribute = type.GetCustomAttribute<ServiceAttribute>();
                    var serviceRegisterType = serviceAttribute?.RegisterType ?? ServiceRegisterType.Auto;
                    if (serviceRegisterType.Equals(ServiceRegisterType.Auto))
                    {
                        continue;
                    }

                    var serviceType = serviceAttribute?.ServiceType ?? ServiceType.Singleton;
                    switch (serviceType)
                    {
                        case ServiceType.Transit:
                            services.AddTransient(@interface);
                            break;
                        case ServiceType.Singleton:
                            services.AddSingleton(@interface);
                            break;
                        default:
                            services.AddSingleton(@interfaces);
                            break;
                    }
                }
            }
        }

        return services;
    }
}
