using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Extensions;
using ProducerRepository = Main.Persistence.Repositories.Producer.ProducerRepository;
using ProductRepository = Main.Persistence.Repositories.Product.ProductRepository;
using SupplierProductRepository = Main.Persistence.Repositories.Product.SupplierProductRepository;

namespace Main.Modules.Product.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddProductPersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<ProductDbContextModule>();

		services.AddDbContext<BulkProductContext>((serviceProvider, options) =>
			options.UseNpgsql(
				serviceProvider.GetRequiredService<UnifiedDbContext>().Database.GetDbConnection()));

		services.AddScoped<IProductRepository, ProductRepository>();
		services.AddScoped<ISupplierProductRepository, SupplierProductRepository>();
		services.AddScoped<IProducerRepository, ProducerRepository>();

		return services;
	}
}
