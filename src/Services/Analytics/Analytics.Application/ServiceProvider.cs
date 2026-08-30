using Abstractions;
using Abstractions.Interfaces;
using Analytics.Application.Configs;
using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Application.Lrts.MarkupCalculation;
using Analytics.Application.Services;
using Analytics.Application.Services.FactSynchronizers;
using Analytics.Entities;
using Application.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces.Currency;
using Application.Common.Services;
using Application.Common.Services.Currency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace Analytics.Application;

public static class ServiceProvider
{
	public static IServiceCollection AddApplicationLayer(
		this IServiceCollection collection,
		IConfiguration? configuration)
	{
		SortByConfig.Configure();
		CursorConfig.Configure();
		collection
			.AddApplicationBase(
				ServicesDefinitions.Analytics,
				configuration,
				typeof(CurrencyRatesProvider).Assembly)
			.AddNamedObjects()
			.AddLrtLayer(typeof(MarkupCalculationLrt).Assembly)
			.AddFusionCache()
			.WithRegisteredDistributedCache()
			.WithRegisteredBackplane()
			.WithSystemTextJsonSerializer();

		collection.RegisterSettingsService();
		collection.AddSingleton<IJsonSerializer, JsonSerializer>();
		collection.AddScoped<ICurrencyConverter, CurrencyConverter>();
		collection.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();
		collection.AddScoped<IFactSynchronizer<PurchasesFact, Guid>, PurchaseFactSynchronizer>();
		collection.AddScoped<ISaleFactSynchronizer, SaleFactSynchronizer>();

		return collection;
	}
}
