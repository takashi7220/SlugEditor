namespace SlugEditor.Core.Service;

public enum ServiceType
{
    Singleton,
    Transit
}

public enum ServiceRegisterType
{
    Manual,
    Auto,
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ServiceAttribute : Attribute
{
    public ServiceType ServiceType { get; }

    public ServiceRegisterType RegisterType { get; }

    public ServiceAttribute(ServiceType serviceType, ServiceRegisterType registerType)
    {
        ServiceType = serviceType;
        RegisterType = registerType;
    }
}
