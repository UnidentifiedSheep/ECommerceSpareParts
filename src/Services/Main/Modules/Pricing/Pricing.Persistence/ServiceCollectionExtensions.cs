using Domain.CommonEntities.Job;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Context;
using Persistence.Extensions;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Offers;
using Pricing.Persistence.Repositories;

namespace Pricing.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPricingPersistence(
		this IServiceCollection collection,
		IConfiguration configuration)
	{
		collection.AddDbContextModule<PricingDbContextModule>();
		collection.AddPersistenceBase<UnifiedDbContext>(
			typeof(BasicEfRepository<,>),
			typeof(BasicLinqRepository<,>),
			typeof(ReadRepository<,>),
			typeof(PriceOffer).Assembly,
			typeof(JobSchedule).Assembly);

		collection.AddScoped<IPriceOfferRepository, PriceOfferRepository>();
		collection.AddScoped<IProductPriceOptionRepository, ProductPriceOptionRepository>();
		collection.AddScoped<IPriceOfferRefreshStateRepository, PriceOfferRefreshStateRepository>();

		return collection;
	}
}
