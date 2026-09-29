using Integrations.Common;
using Integrations.ExchangeRate.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Integrations.ExchangeRate.Di;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddExchangeRates(this IServiceCollection services)
	{
		services.AddHttpClient<IExchangeRateClient, ExchangeRate.Cbr.Client>((_, client) =>
		{
			client.BaseAddress = new Uri("https://www.cbr-xml-daily.ru/latest.js");
		})
		.AddDefaultResilenceHandler();

		services.AddHttpClient<IExchangeRateClient, ExchangeRate.MoneyConvert.Client>((_, client) =>
		{
			client.BaseAddress = new Uri("https://cdn.moneyconvert.net/api/latest.json");
		})
		.AddDefaultResilenceHandler();

		services.AddTransient<IExchangeRateClientFactory, ExchangeRateClientFactory>();
		return services;
	}
}
