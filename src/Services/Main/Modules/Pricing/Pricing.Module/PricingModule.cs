using Application.Common.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application;
using Pricing.Persistence;

namespace Pricing.Module;

public sealed class PricingModule : ModuleBase
{
	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddPricingApplication(configuration);
		services.AddPricingPersistence(configuration);
	}
}
