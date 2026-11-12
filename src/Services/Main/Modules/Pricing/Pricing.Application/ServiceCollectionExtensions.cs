using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pricing.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPricingApplication(
		this IServiceCollection services,
		IConfiguration configuration)
		=> services;
}
