using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pricing.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPricingPersistence(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		return services;
	}
}
