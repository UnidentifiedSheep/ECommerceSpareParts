using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Localization.Domain.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddLocalization(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services
			.AddOptions<LocalesOptions>()
			.Bind(configuration.GetRequiredSection(LocalesOptions.SectionName))
			.ValidateDataAnnotations()
			.Validate(
				x => x.Supported.Length > 0,
				$"{nameof(LocalesOptions.Supported)} must contain at least one locale")
			.Validate(
				x => x.Supported.Distinct(StringComparer.OrdinalIgnoreCase).Count() == x.Supported.Length,
				$"{nameof(LocalesOptions.Supported)} must not contain duplicates")
			.Validate(
				x => x.Supported.Contains(x.Default, StringComparer.OrdinalIgnoreCase),
				$"{nameof(LocalesOptions.Default)} must be included in {nameof(LocalesOptions.Supported)}")
			.ValidateOnStart();

		var options = configuration.GetRequiredSection(LocalesOptions.SectionName).Get<LocalesOptions>() ??
			throw new InvalidOperationException(
				$"Missing {LocalesOptions.SectionName} configuration section");

		return services.RegisterLocalization(
			options.Supported.Select(x => (Locale)x).ToArray());
	}

	public static IServiceCollection AddLocalization(
		this IServiceCollection services,
		Locale defaultLocale,
		params Locale[] locales)
	{
		services.AddSingleton(
			Options.Create(
				new LocalesOptions
				{
					Default = defaultLocale, Supported = locales.Select(x => x.ToString()).ToArray()
				}));

		return services.RegisterLocalization(locales);
	}

	private static IServiceCollection RegisterLocalization(
		this IServiceCollection services,
		params Locale[] locales)
	{
		services.AddLocales(locales).AddStringLocalizer().AddContextualStringLocalizer();

		return services;
	}

	public static IServiceCollection AddLocales(this IServiceCollection services, params Locale[] locales)
	{
		foreach (var locale in locales)
			services.AddSingleton<ILocalizerContainer, LocalizerContainer>(_ =>
				new LocalizerContainer(locale));

		return services;
	}

	public static IServiceCollection AddStringLocalizer<TLocalizer>(this IServiceCollection services)
		where TLocalizer : class, IStringLocalizer
	{
		services.AddSingleton<IStringLocalizer, TLocalizer>();
		return services;
	}

	public static IServiceCollection AddStringLocalizer(this IServiceCollection services) =>
		services.AddStringLocalizer<StringLocalizer>();

	public static IServiceCollection AddContextualStringLocalizer<TLocalizer>(
		this IServiceCollection services) where TLocalizer : class, IContextualStringLocalizer
	{
		services.AddSingleton<IContextualStringLocalizer, TLocalizer>();
		return services;
	}

	public static IServiceCollection AddContextualStringLocalizer(this IServiceCollection services)
	{
		return services.AddContextualStringLocalizer<ContextualStringLocalizer>();
	}
}
