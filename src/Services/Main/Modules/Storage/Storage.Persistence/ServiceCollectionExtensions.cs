using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Storage.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStoragePersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<StorageDbContextModule>();

		return services;
	}
}
