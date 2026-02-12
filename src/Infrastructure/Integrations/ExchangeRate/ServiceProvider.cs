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
        services.AddHttpClient(nameof(ExchangeRateProvider.Cbr), client =>
        {
            client.BaseAddress = new Uri("https://www.cbr-xml-daily.ru/latest.js");
        });
        
        services.AddHttpClient(nameof(ExchangeRateProvider.MoneyConvert), client =>
        {
            client.BaseAddress = new Uri("https://cdn.moneyconvert.net/api/latest.json");
        });

        services.AddTransient<IExchangeRateClient, CbrClient>();
        services.AddTransient<IExchangeRateClient, MoneyConvertClient>();

        services.AddTransient<IExchangeRateClientFactory, ExchangeRateClientFactory>();
        return services;
    }
}