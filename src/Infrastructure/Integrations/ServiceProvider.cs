using System.Net.Http.Headers;
using Core.Interfaces;
using Core.Interfaces.Integrations;
using Integrations.Models.ExchangeRates;
using Integrations.TimeWebCloud;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integrations;

public static class ServiceProvider
{
    public static IServiceCollection AddIntegrations(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddHttpClient();
        collection.AddHttpClient("TimewebClient", sp =>
        {
            sp.BaseAddress = new Uri(configuration.GetValue<string>("TimeWebConnect:BaseUrl")!);
            sp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                configuration.GetValue<string>("TimeWebConnect:Token")!);
        });

        collection.Configure<ExchangeRatesOptions>(configuration.GetSection("Exchange"));

        collection.AddScoped<ITimeWebMail, TimeWebMail>();
        collection.AddScoped<IExchangeRates, ExchangeRates.ExchangeRates>();

        return collection;
    }
}