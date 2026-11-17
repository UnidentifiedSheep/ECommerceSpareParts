using Main.Modules.Sale.Application.Interfaces.Persistence;
using Main.Modules.Sale.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Extensions;

namespace Main.Modules.Sale.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSalePersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<SaleDbContextModule>()
			.AddEntities(typeof(Entities.Sale.Sale.Sale).Assembly);
		services.AddScoped<ISaleRepository, SaleRepository>();

		return services;
	}
}
