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
		new TModule().ConfigureServices(services, configuration);
		return services;
	}
}
