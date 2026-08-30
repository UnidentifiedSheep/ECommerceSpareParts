using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Cache;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplicationCache(this IServiceCollection services)
	{
		services.AddScoped<IProductProvider, ProductProvider>();
		services.AddScoped<IProductCacheInvalidator, ProductCacheInvalidator>();
		services.AddScoped<IUserCacheRepository, UserCacheRepository>();
		services.AddScoped<ICurrencyCacheRepository, CurrencyCacheRepository>();
		services.AddScoped<IOneTimeTokenStore, OneTimeTokenStore>();

		return services;
	}
}
