using System.Globalization;
using Localization.Domain;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace Api.Common.Localization;

internal sealed class ConfiguredRequestLocalizationOptions(
	IOptions<LocalesOptions> localesOptions) : IConfigureOptions<RequestLocalizationOptions>
{
	public void Configure(RequestLocalizationOptions options)
	{
		var locales = localesOptions.Value;
		var supportedCultures = locales.Supported
			.Select(CultureInfo.GetCultureInfo)
			.ToArray();

		options.DefaultRequestCulture = new RequestCulture(locales.Default);
		options.SupportedCultures = supportedCultures;
		options.SupportedUICultures = supportedCultures;
		options.RequestCultureProviders =
		[
			new AcceptLanguageHeaderRequestCultureProvider()
		];
	}
}
