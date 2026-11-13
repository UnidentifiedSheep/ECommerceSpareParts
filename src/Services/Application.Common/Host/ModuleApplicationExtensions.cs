using System.Reflection;
using Application.Common.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Host;

public static class ModuleApplicationExtensions
{
	private static readonly Type[] MediatRHandlerTypes =
	[
		typeof(IRequestHandler<,>),
		typeof(IRequestHandler<>),
		typeof(INotificationHandler<>),
		typeof(IStreamRequestHandler<,>)
	];

	/// <summary>
	///     Registers everything the host normally discovers by scanning its own application assembly
	///     (MediatR handlers, validators, cache policies, projections) for a module assembly.
	///     Pipeline behaviors are registered once by the host and are not duplicated here.
	/// </summary>
	public static IServiceCollection AddModuleApplication(
		this IServiceCollection services,
		Assembly assembly)
	{
		ArgumentNullException.ThrowIfNull(assembly);

		services.Scan(scan => scan
			.FromAssemblies(assembly)
			.AddClasses(classes => classes.AssignableToAny(MediatRHandlerTypes))
			.AsImplementedInterfaces(type => type.IsGenericType &&
				MediatRHandlerTypes.Contains(type.GetGenericTypeDefinition()))
			.WithTransientLifetime());

		return services
			.RegisterCachePolicies(assembly)
			.RegisterDbValidations(assembly)
			.RegisterFluentValidations(assembly)
			.RegisterProjectionProviders(assembly);
	}
}
