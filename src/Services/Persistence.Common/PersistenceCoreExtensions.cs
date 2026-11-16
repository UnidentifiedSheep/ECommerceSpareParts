using System.Reflection;
using Application.Common.Host;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common.Repositories;
using Persistence.Context;
using Persistence.Extensions;

namespace Persistence.Common;

public static class PersistenceCoreExtensions
{
	public static IServiceCollection AddPersistenceCore(this IServiceCollection services)
	{
		services.AddDbContextModule<CoreDbContextModule>();
		services.AddPersistenceBase<UnifiedDbContext>(
			services.GetValidationErrorsRegistry().Apply,
			typeof(BasicEfRepository<,>),
			typeof(BasicLinqRepository<,>),
			typeof(ReadRepository<,>));

		return services;
	}

	public static IServiceCollection AddEntities(this IServiceCollection services, params Assembly[] assemblies)
	{
		var tracker = services.GetEntityAssemblyTracker();
		var fresh = assemblies.Where(tracker.Add).ToArray();
		if (fresh.Length == 0)
			return services;

		RepositoriesExtensions.AddRepositories(
			services,
			typeof(BasicEfRepository<,>),
			typeof(BasicLinqRepository<,>),
			typeof(ReadRepository<,>),
			fresh);

		return services;
	}

	private static HashSet<Assembly> GetEntityAssemblyTracker(this IServiceCollection services)
	{
		if (services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(EntityAssemblyTracker))?
				.ImplementationInstance is EntityAssemblyTracker existing)
			return existing.Assemblies;

		var tracker = new EntityAssemblyTracker();
		services.AddSingleton(tracker);
		return tracker.Assemblies;
	}

	private sealed class EntityAssemblyTracker
	{
		public HashSet<Assembly> Assemblies { get; } = [];
	}
}
