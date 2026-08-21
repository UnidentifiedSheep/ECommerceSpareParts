using Application.Common.Interfaces.Domains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.Common.Extensions;

public static class CommonDomainServiceCollectionExtensions
{
    public static IServiceCollection AddCommonDomain<TDomain>(
        this IServiceCollection services)
        where TDomain : ICommonDomain
    {
        services.TryAddSingleton<ICommonDomainMarker<TDomain>>(
            _ => new CommonDomainMarker<TDomain>());

        return services;
    }

    private sealed class CommonDomainMarker<TDomain>
        : ICommonDomainMarker<TDomain>
        where TDomain : ICommonDomain;
}
