using Main.Application.Interfaces.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;
using StorageRouteRepository = Main.Persistence.Repositories.Storage.StorageRouteRepository;

namespace Main.Modules.Storage.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStoragePersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<StorageDbContextModule>();
		services.AddScoped<IStorageRouteRepository, StorageRouteRepository>();

		return services;
	}
}
