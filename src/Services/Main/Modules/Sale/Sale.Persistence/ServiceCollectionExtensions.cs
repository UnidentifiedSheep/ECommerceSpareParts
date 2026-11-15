using Main.Application.Interfaces.Persistence;
using Main.Persistence.Repositories.Sale;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Sale.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSalePersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<SaleDbContextModule>();
		services.AddScoped<ISaleRepository, SaleRepository>();

		return services;
	}
}
