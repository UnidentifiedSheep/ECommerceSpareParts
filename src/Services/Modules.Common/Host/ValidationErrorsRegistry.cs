using BulkValidation.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Host;

public sealed class ValidationErrorsRegistry
{
	private readonly List<Action<ValidationErrors>> _configurations = [];
	private ValidationErrors? _target;

	public void Add(Action<ValidationErrors> configuration)
	{
		if (_target is not null)
			throw new InvalidOperationException(
				"Validation errors must be registered before the persistence core is added.");

		_configurations.Add(configuration);
	}

	public void Apply(ValidationErrors errors)
	{
		_target = errors;
		foreach (var configuration in _configurations)
			configuration(errors);
	}
}

public static class ValidationErrorsServiceCollectionExtensions
{
	public static IServiceCollection AddValidationErrors(
		this IServiceCollection services,
		Action<ValidationErrors> configuration)
	{
		services.GetValidationErrorsRegistry().Add(configuration);
		return services;
	}

	public static ValidationErrorsRegistry GetValidationErrorsRegistry(this IServiceCollection services)
	{
		if (services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ValidationErrorsRegistry))?
				.ImplementationInstance is ValidationErrorsRegistry existing)
			return existing;

		var registry = new ValidationErrorsRegistry();
		services.AddSingleton(registry);
		return registry;
	}
}
