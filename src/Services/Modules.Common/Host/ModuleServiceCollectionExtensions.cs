using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Host;

public static class ModuleServiceCollectionExtensions
{
	public static IServiceCollection AddModule<TModule>(
		this IServiceCollection services,
		IConfiguration configuration)
		where TModule : ModuleBase, new()
	{
		var module = new TModule();
		if (services.GetModuleCatalog().Add(module))
			module.ConfigureServices(services, configuration);

		return services;
	}

	public static ModuleCatalog GetModuleCatalog(this IServiceCollection services)
	{
		if (services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ModuleCatalog))?
				.ImplementationInstance is ModuleCatalog existing)
			return existing;

		var catalog = new ModuleCatalog();
		services.AddSingleton(catalog);
		return catalog;
	}
}
