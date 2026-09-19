using Analytics.Application.Interfaces.Repositories;
using Analytics.Entities;
using Analytics.Persistence.Context;
using Analytics.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;

namespace Analytics.Persistence;

public static class ServiceProvider
{
	public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection)
	{
		collection.AddPersistenceBase<DContext>(
			typeof(BasicEfRepository<,>),
			typeof(BasicLinqRepository<,>),
			typeof(ReadRepository<,>),
			typeof(SalesFact).Assembly);

		collection.AddScoped<ISaleFactRepository, SaleFactRepository>();

		return collection;
	}
}
