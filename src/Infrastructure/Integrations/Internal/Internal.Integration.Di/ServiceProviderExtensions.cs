using Internal.Integration.Auth;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Main;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Di;

public static class ServiceProviderExtensions
{
    private const string InternalTokenHeader = "X-Internal-Token";

    public static IServiceCollection AddIntegrationClients(
        this IServiceCollection services)
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

        services.AddHttpClient<IAuthClient, CacheableAuthClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<InternalServicesOptions>>().Value;
            client.BaseAddress = new Uri(options.Main.Url);
            client.DefaultRequestHeaders.Add(InternalTokenHeader, options.InternalToken);
        });

        services.AddHttpClient<IMainClient, MainClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<InternalServicesOptions>>().Value;
            client.BaseAddress = new Uri(options.Main.Url);
            client.DefaultRequestHeaders.Add(InternalTokenHeader, options.InternalToken);
        });

        return services;
    }
}
