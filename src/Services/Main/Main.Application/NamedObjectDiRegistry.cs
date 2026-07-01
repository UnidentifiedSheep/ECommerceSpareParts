using Application.Common.Extensions;
using Application.Common.Handlers.NamedObjects;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;
using Main.Application.NamedObjects;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Main.Application;

public static class NamedObjectDiRegistry
{
    public static IServiceCollection AddNamedObjects(this IServiceCollection services)
    {
        services.AddSingleton<INamedObjectGroupRegistry, NamedObjectGroupRegistry>();
        
        services.TryAddScoped<
            IRequestHandler<GetNamedObjectsQuery, GetNamedObjectsResult>,
            GetNamedObjectsHandler>();
        
        return services
            .RegisterNamedObject<StorageContentExtractPolicyBase>(objectsLifetime: ServiceLifetime.Singleton)
            .RegisterNamedObject<SettingDefinitionNamedObjectBase>(
                assembly: typeof(StorageContentExtractPolicyBase).Assembly, 
                objectsLifetime: ServiceLifetime.Scoped);
    }
}