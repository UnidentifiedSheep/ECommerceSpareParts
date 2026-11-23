using Application.Common.Host;
using Application.Common.Interfaces;
using Application.Common.NamedObject;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Currency.Abstractions.Models;
using Main.Modules.Currency.Application.Cache;
using Main.Modules.Currency.Application.Configs;
using Main.Modules.Currency.Application.Interfaces;
using Main.Modules.Currency.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Currency.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCurrencyApplication(this IServiceCollection services)
	{
		services.AddValidationErrors(CurrencyValidationConfiguration.Configure);

		var assembly = CurrencyApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly)
			.RegisterNamedObject<SettingDefinitionNamedObjectBase>(assembly);

		services.AddScoped<ICurrencyChecks, CurrencyChecks>();
		services.AddScoped<ICurrencyLookup, CurrencyLookup>();
		services.AddScoped<IDisplay<int, CurrencyDisplayInfo>, CurrencyDisplay>();
		services.AddScoped<ICurrencyConverter, CurrencyConverter>();
		services.AddScoped<ICurrencyRatesProvider, CurrencyRatesProvider>();
		services.AddScoped<ICurrencyRateUpdater, CurrencyRateUpdater>();
		services.AddSingleton<ICurrencyRateSnapshotProvider, CurrencyRateSnapshotProvider>();
		services.AddScoped<ICurrencyCacheRepository, CurrencyCacheRepository>();

		return services;
	}
}
