using Internal.Integration.Auth;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Main;
using Microsoft.Extensions.DependencyInjection;

namespace Internal.Integration.Di;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddIntegrationClients(
        IServiceCollection services)
    {
        services.AddOptions<InternalServicesOptions>()
            .BindConfiguration(InternalServicesOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        services.AddOptions<InternalServiceCredentials>()
            .BindConfiguration(InternalServiceCredentials.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IAuthTokenCache, AuthTokenCache>();
        
        services.AddHttpClient<IAuthClient, CacheableAuthClient>();
        services.AddHttpClient<IMainClient, MainClient>();
        
        return services;
    }
}