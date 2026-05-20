using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;
using Search.Abstractions.Options;

namespace Search.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services)
    {
        services.AddSingleton<IOpenSearchClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OpenSearchOptions>>().Value;
            var connectionSettings = new ConnectionSettings(new Uri(options.Uri));

            if (!string.IsNullOrWhiteSpace(options.Username) &&
                !string.IsNullOrWhiteSpace(options.Password))
            {
                connectionSettings = connectionSettings.BasicAuthentication(
                    options.Username,
                    options.Password);
            }

            if (options.AllowInvalidCertificate)
            {
                connectionSettings = connectionSettings.ServerCertificateValidationCallback(
                    CertificateValidations.AllowAll);
            }

            return new OpenSearchClient(connectionSettings);
        });

        return services;
    }
}
