using System.Reflection;
using Application.Common.Host;
using Main.Modules.Currency.Application;
using Main.Modules.Currency.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Currency;

public sealed class CurrencyModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies =>
	[
		CurrencyApplicationAssembly.Value
	];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddCurrencyApplication();
		services.AddCurrencyPersistence();
	}
}
