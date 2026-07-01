using Application.Common.Extensions;
using Application.Common.Handlers.NamedObjects;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pricing.Application.NamedObjects;

namespace Pricing.Application;

public static class NamedObjectDiRegistry
{
    public static IServiceCollection AddNamedObjects(this IServiceCollection services)
    {
        services.AddSingleton<INamedObjectGroupRegistry, NamedObjectGroupRegistry>();
        
        services.TryAddScoped<
            IRequestHandler<GetNamedObjectsQuery, GetNamedObjectsResult>,
            GetNamedObjectsHandler>();
        
        return services
            .RegisterNamedObject<SettingDefinitionNamedObjectBase>(objectsLifetime: ServiceLifetime.Scoped);
    }
}