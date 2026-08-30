using System.Globalization;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Microsoft.Extensions.Options;

namespace Localization.Domain;

public sealed class ContextualStringLocalizer(
	IStringLocalizer stringLocalizer,
	IOptions<LocalesOptions>? localeOptions = null) : IContextualStringLocalizer
{
	private readonly Locale? _defaultLocale = GetDefaultLocale(localeOptions);

	public Locale Locale
	{
		get
		{
			var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

			if (!string.IsNullOrWhiteSpace(language))
			{
				var locale = new Locale(language);
				if (stringLocalizer.IsSupported(locale))
					return locale;
			}

			return _defaultLocale ?? throw new InvalidOperationException("Unable to resolve current locale.");
		}
	}

	public string Get(string key) => stringLocalizer.Get(key, Locale);

	public string Get(string key, params object[] arguments) => stringLocalizer.Get(
		key,
		Locale,
		arguments);

	public bool TryGet(string key, out string? value) => stringLocalizer.TryGet(
		key,
		Locale,
		out value);

	public bool TryGet(
		string key,
		out string? value,
		params object[] arguments) => stringLocalizer.TryGet(
		key,
		Locale,
		out value,
		arguments);

	public string? GetOrDefault(string key) => TryGet(key, out var value) ? value : null;

	public string? GetOrDefault(string key, params object[] arguments) => TryGet(
		key,
		out var value,
		arguments)
		? value
		: null;

	public string this[string key] => Get(key);

	private static Locale? GetDefaultLocale(IOptions<LocalesOptions>? localeOptions)
	{
		var defaultLocale = localeOptions?.Value.Default;

		return string.IsNullOrWhiteSpace(defaultLocale) ? default : new Locale(defaultLocale);
	}
}
