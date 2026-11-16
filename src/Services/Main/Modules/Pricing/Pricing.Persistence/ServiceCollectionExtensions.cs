using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Persistence.Repositories;

namespace Pricing.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPricingPersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<PricingDbContextModule>().AddEntities(typeof(Pricing.Entities.Offers.PriceOffer).Assembly);

		services.AddScoped<IPriceOfferRepository, PriceOfferRepository>();
		services.AddScoped<IProductPriceOptionRepository, ProductPriceOptionRepository>();
		services.AddScoped<IPriceOfferRefreshStateRepository, PriceOfferRefreshStateRepository>();

		return services;
	}
}
