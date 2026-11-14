using Domain.CommonEntities.Job;
using Main.Application.Configs;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product;
using Main.Persistence.Context;
using Main.Persistence.Repositories;
using Main.Persistence.Repositories.Sale;
using Main.Persistence.Repositories.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Core.Entities;
using Notification.Persistence;
using Persistence.Common;
using Persistence.Context;
using Persistence.Extensions;
using UserRepository = Main.Persistence.Repositories.User.UserRepository;

namespace Main.Persistence;

public static class ServiceProvider
{
	public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection)
	{
		collection.AddDbContextModule<MainDbContextModule>();
		collection.AddPersistenceBase<UnifiedDbContext>(
			ValidationConfiguration.Configure,
			typeof(BasicEfRepository<,>),
			typeof(BasicLinqRepository<,>),
			typeof(ReadRepository<,>),
			typeof(Main.Entities.User.User).Assembly,
			typeof(Product).Assembly,
			typeof(Main.Entities.Currency.Currency).Assembly,
			typeof(InAppNotification).Assembly,
			typeof(JobSchedule).Assembly);
		collection.AddNotificationPersistence<UnifiedDbContext>();

		collection.AddScoped<IStorageRouteRepository, StorageRouteRepository>();
		collection.AddScoped<IUserRepository, UserRepository>();
		collection.AddScoped<ISaleRepository, SaleRepository>();

		return collection;
	}
}
