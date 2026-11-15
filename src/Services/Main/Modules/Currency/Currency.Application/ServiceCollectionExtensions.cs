using Application.Common.Host;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Lrt;
using Application.Common.NamedObject;
using Application.Common.Services.Currency;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services.Currency;
using Main.Application.Services;
using Main.Application.Services.Currency;
using Main.Modules.Currency.Abstractions;
using Main.Cache;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Currency.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCurrencyApplication(this IServiceCollection services)
	{
		var assembly = CurrencyApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly)
			.RegisterNamedObject<SettingDefinitionNamedObjectBase>(assembly);

		services.AddScoped<ICurrencyChecks, CurrencyChecks>();
		services.AddScoped<ICurrencyLookup, CurrencyLookup>();
		services.AddScoped<ICurrencyConverter, CurrencyConverter>();
		services.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();
		services.AddScoped<ICurrencyRateUpdater, CurrencyRateUpdater>();
		services.AddSingleton<ICurrencyRateSnapshotProvider, CurrencyRateSnapshotProvider>();
		services.AddScoped<ICurrencyCacheRepository, CurrencyCacheRepository>();

		return services;
	}
}
