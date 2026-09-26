using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject.Core.Interfaces;

namespace NamedObject;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection RegisterNamedObject<TBaseObject>(
		this IServiceCollection services,
		Assembly? assembly = null,
		ServiceLifetime objectsLifetime = ServiceLifetime.Scoped,
		params Type[] objectsToExclude) where TBaseObject : class, INamedObject
	{
		assembly ??= typeof(TBaseObject).Assembly;
		var excludedTypes = objectsToExclude.ToHashSet();

		services.Scan(scan =>
		{
			var registration = scan
				.FromAssemblies(assembly)
				.AddClasses(classes => classes
					.AssignableTo<TBaseObject>()
					.Where(type => !excludedTypes.Contains(type)))
				.As<TBaseObject>();

			switch (objectsLifetime)
			{
				case ServiceLifetime.Singleton:
					registration.WithSingletonLifetime();
					break;

				case ServiceLifetime.Scoped:
					registration.WithScopedLifetime();
					break;

				case ServiceLifetime.Transient:
					registration.WithTransientLifetime();
					break;

				default:
					throw new ArgumentOutOfRangeException(
						nameof(objectsLifetime),
						objectsLifetime,
						null);
			}
		});

		services.AddNamedObjectRegistry();

		return services;
	}

	public static IServiceCollection AddNamedObjectRegistry(this IServiceCollection services)
	{
		services.TryAddScoped(typeof(INamedObjectRegistry<>), typeof(NamedObjectRegistry<>));
		services.TryAddScoped<INamedObjectGroupResolver, NamedObjectGroupResolver>();

		return services;
	}
}
