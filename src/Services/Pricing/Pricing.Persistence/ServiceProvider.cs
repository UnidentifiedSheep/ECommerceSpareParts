using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Persistence.Contexts;
using Pricing.Persistence.Repositories;

namespace Pricing.Persistence;

public static class ServiceProvider
{
	public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection)
	{
		collection.AddPersistenceBase<DContext>(typeof(BasicEfRepository<,>), typeof(ReadRepository<,>));

		collection.AddScoped<IPriceOfferRepository, PriceOfferRepository>();
		collection.AddScoped<IProductPriceOptionRepository, ProductPriceOptionRepository>();
		collection.AddScoped<IPriceOfferRefreshStateRepository, PriceOfferRefreshStateRepository>();

		return collection;
	}
}
