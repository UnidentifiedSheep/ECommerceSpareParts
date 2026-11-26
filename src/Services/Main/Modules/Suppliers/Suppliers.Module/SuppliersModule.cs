using System.Reflection;
using Application.Common.Host;
using Integrations.Supplier.DI;
using Main.Modules.Suppliers.Application;
using Main.Modules.Suppliers.Application.Services.Armtek;
using Main.Modules.Suppliers.Application.Services.Favorite;
using Main.Modules.Suppliers.Application.Services.Tmtr;
using Main.Modules.Suppliers.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Suppliers;

public sealed class SuppliersModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [SuppliersApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddSuppliersApplication();
		services
			.AddFavoriteIntegration<FavoriteConnectionProvider, FavoriteSettingsProvider>()
			.AddTmtrIntegration<TmtrConnectionProvider, TmtrSettingsProvider>()
			.AddArmtekIntegration<ArmtekConnectionProvider>();
		services.AddSuppliersPersistence();
	}
}
