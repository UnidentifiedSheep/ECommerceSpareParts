using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Main.Persistence.Repositories;
using Main.Persistence.Repositories.Balance;
using Main.Persistence.Repositories.Currency;
using Main.Persistence.Repositories.Product;
using Main.Persistence.Repositories.Sale;
using Main.Persistence.Repositories.Storage;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using ProducerRepository = Main.Persistence.Repositories.Producer.ProducerRepository;
using StorageContentRepository = Main.Persistence.Repositories.Storage.StorageContentRepository;
using UserRepository = Main.Persistence.Repositories.User.UserRepository;

namespace Main.Persistence;

public static class ServiceProvider
{
	public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection)
	{
		collection.AddPersistenceBase<DContext>(typeof(BasicEfRepository<,>), typeof(ReadRepository<,>));

		collection.AddScoped<IProductRepository, ProductRepository>();
		collection.AddScoped<ISupplierProductRepository, SupplierProductRepository>();
		collection.AddScoped<IProducerRepository, ProducerRepository>();
		collection.AddScoped<IStorageRouteRepository, StorageRouteRepository>();
		collection.AddScoped<IStorageContentRepository, StorageContentRepository>();
		collection.AddScoped<IProductReservationRepository, ProductReservationRepository>();
		collection.AddScoped<IUserRepository, UserRepository>();
		collection.AddScoped<ITransactionRepository, TransactionRepository>();
		collection.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();
		collection.AddScoped<ICurrencyRepository, CurrencyRepository>();
		collection.AddScoped<ISaleRepository, SaleRepository>();

		return collection;
	}
}
