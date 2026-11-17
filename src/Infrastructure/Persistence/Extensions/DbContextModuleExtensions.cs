using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Interfaces;

namespace Persistence.Extensions;

public static class DbContextModuleExtensions
{
	public static IServiceCollection AddDbContextModule<TModule>(this IServiceCollection services)
		where TModule : class, IDbContextModule
	{
		services.TryAddEnumerable(ServiceDescriptor.Singleton<IDbContextModule, TModule>());
		return services;
	}
}
