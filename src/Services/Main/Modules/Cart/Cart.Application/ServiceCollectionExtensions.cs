using Application.Common.Host;
using Main.Modules.Cart.Application.Configs;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Cart.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCartApplication(this IServiceCollection services)
	{
		services.AddValidationErrors(CartValidationConfiguration.Configure);

		return services.AddModuleApplication(CartApplicationAssembly.Value);
	}
}
