using System.Reflection;
using Application.Common.Interfaces.Lrt;
using Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Extensions;

public static class JobProviderServiceCollectionExtensions
{
	public static IServiceCollection
		RegisterJobProviders<TAssemblyMarker>(this IServiceCollection services) =>
		services.RegisterJobProviders(typeof(TAssemblyMarker).Assembly);

	public static IServiceCollection RegisterJobProviders(
		this IServiceCollection services,
		params Assembly[] assemblies)
	{
		ArgumentNullException.ThrowIfNull(assemblies);
		if (assemblies.Length == 0)
			throw new ArgumentException("At least one assembly is required.", nameof(assemblies));

		var jobProviderType = typeof(IJobProvider<,>);
		var registrations = new Dictionary<Type, (Type Implementation, ServiceLifetime Lifetime)>();

		foreach (var implementation in assemblies
					.Distinct()
					.SelectMany(x => x.DefinedTypes)
					.Where(x => x is { IsClass: true, IsAbstract: false }))
		{
			var servicesToRegister = implementation
				.ImplementedInterfaces
				.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == jobProviderType)
				.ToList();

			if (servicesToRegister.Count == 0)
				continue;

			var lifetime = implementation.GetCustomAttribute<LifetimeAttribute>()?.Lifetime ??
				throw new InvalidOperationException(
					$"Job provider {implementation.FullName} must have " + $"{nameof(LifetimeAttribute)}.");

			foreach (var service in servicesToRegister)
			{
				if (registrations.TryAdd(service, (implementation.AsType(), ToServiceLifetime(lifetime))))
					continue;

				var existing = registrations[service].Implementation;
				throw new InvalidOperationException(
					$"Multiple job providers are registered for {service}: " +
					$"{existing.FullName}, {implementation.FullName}.");
			}
		}

		foreach (var (service, registration) in registrations)
			services.Add(
				new ServiceDescriptor(
					service,
					registration.Implementation,
					registration.Lifetime));

		return services;
	}

	private static ServiceLifetime ToServiceLifetime(Lifetime lifetime) => lifetime switch
	{
		Lifetime.Singleton => ServiceLifetime.Singleton,
		Lifetime.Scoped => ServiceLifetime.Scoped,
		Lifetime.Transient => ServiceLifetime.Transient,
		_ => throw new ArgumentOutOfRangeException(
			nameof(lifetime),
			lifetime,
			null)
	};
}
