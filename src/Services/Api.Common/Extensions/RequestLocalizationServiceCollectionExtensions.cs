using Api.Common.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Api.Common.Extensions;

public static class RequestLocalizationServiceCollectionExtensions
{
	public static IServiceCollection AddConfiguredRequestLocalization(
		this IServiceCollection services)
	{
		services.TryAddEnumerable(
			ServiceDescriptor.Singleton<
				IConfigureOptions<RequestLocalizationOptions>,
				ConfiguredRequestLocalizationOptions>());

		return services;
	}
}
