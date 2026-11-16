using Main.Entities.Storage.Storage;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Extensions;

namespace Main.Modules.Storage.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStoragePersistence(this IServiceCollection services)
	{
		services.AddDbContextModule<StorageDbContextModule>().AddEntities(typeof(StorageContent).Assembly);

		return services;
	}
}
