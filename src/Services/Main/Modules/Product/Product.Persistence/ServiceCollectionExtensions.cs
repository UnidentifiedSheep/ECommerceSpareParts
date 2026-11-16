using Main.Modules.Product.Application.Interfaces.Persistence;
using Main.Modules.Product.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Context;
using Persistence.Extensions;
using ProducerRepository = Main.Modules.Product.Persistence.Repositories.Producer.ProducerRepository;
using ProductRepository = Main.Modules.Product.Persistence.Repositories.Product.ProductRepository;
using SupplierProductRepository =
	Main.Modules.Product.Persistence.Repositories.Product.SupplierProductRepository;

namespace Main.Modules.Product.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddProductPersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<ProductDbContextModule>()
			.AddEntities(typeof(Entities.Product.Product.Product).Assembly);

		services.AddDbContext<BulkProductContext>((serviceProvider, options) =>
			options.UseNpgsql(
				serviceProvider.GetRequiredService<UnifiedDbContext>().Database.GetDbConnection()));

		services.AddScoped<IProductRepository, ProductRepository>();
		services.AddScoped<ISupplierProductRepository, SupplierProductRepository>();
		services.AddScoped<IProducerRepository, ProducerRepository>();

		return services;
	}
}
