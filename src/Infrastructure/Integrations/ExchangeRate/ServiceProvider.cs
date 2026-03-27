using Abstractions.Interfaces.Integrations.ExchangeRate;
using Enums;
using ExchangeRate.Clients;
using ExchangeRate.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangeRate;

public static class ServiceProvider
{
    public static IServiceCollection AddExchangeRates(this IServiceCollection services)
    {
        services.AddHttpClient<IExchangeRateClient, CbrClient>((_, client) =>
        {
            client.BaseAddress = new Uri("https://www.cbr-xml-daily.ru/latest.js");
        });
        services.AddHttpClient<IExchangeRateClient, MoneyConvertClient>((_, client) =>
        {
            client.BaseAddress = new Uri("https://cdn.moneyconvert.net/api/latest.json");
        });

        services.AddTransient<IExchangeRateClientFactory, ExchangeRateClientFactory>();
        return services;
    }
}