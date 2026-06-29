using Application.Common.Extensions;
using Application.Common.NamedObject;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Application;

public static class NamedObjectDiRegistry
{
    public static IServiceCollection AddNamedObjects(this IServiceCollection services)
    {
        return services
            .RegisterNamedObject<StorageContentExtractPolicyBase>(objectsLifetime: ServiceLifetime.Singleton)
            .RegisterNamedObject<SettingDefinitionNamedObjectBase>(objectsLifetime: ServiceLifetime.Scoped);
    }
}