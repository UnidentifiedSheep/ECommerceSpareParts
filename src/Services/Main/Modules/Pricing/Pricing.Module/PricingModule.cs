using System.Reflection;
using Application.Common.Host;
using Application.Common.Services.Supplier;
using Application.Common.Services.Supplier.Favorite;
using Application.Common.Services.Supplier.Tmtr;
using Integrations.Supplier.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application;
using Pricing.Persistence;

namespace Pricing.Module;

public sealed class PricingModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [typeof(Global).Assembly];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddPricingApplication();
		services
			.AddMainSupplierSettingProviders()
			.AddFavoriteIntegration<FavoriteConnectionProvider, FavoriteSettingsProvider>()
			.AddTmtrIntegration<TmtrConnectionProvider, TmtrSettingsProvider>();
		services.AddPricingPersistence();
	}
}
