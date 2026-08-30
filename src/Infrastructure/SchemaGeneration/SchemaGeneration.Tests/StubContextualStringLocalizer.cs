using System.Globalization;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace SchemaGeneration.Tests;

internal sealed class StubContextualStringLocalizer(IReadOnlyDictionary<string, string> values)
	: IContextualStringLocalizer
{
	public Locale Locale { get; private set; } = "EN";

	public string this[string key] => Get(key);

	public string Get(string key) => GetOrDefault(key) ?? key;

	public string Get(string key, params object[] arguments) => string.Format(
		CultureInfo.InvariantCulture,
		Get(key),
		arguments);

	public bool TryGet(string key, out string? value) => values.TryGetValue(key, out value);

	public bool TryGet(
		string key,
		out string? value,
		params object[] arguments)
	{
		if (!TryGet(key, out value))
			return false;
		value = string.Format(
			CultureInfo.InvariantCulture,
			value!,
			arguments);
		return true;
	}

	public string? GetOrDefault(string key) => values.GetValueOrDefault(key);

	public string? GetOrDefault(string key, params object[] arguments)
	{
		var value = GetOrDefault(key);
		return value is null
			? null
			: string.Format(
				CultureInfo.InvariantCulture,
				value,
				arguments);
	}

	public void SetLocale(Locale locale) => Locale = locale;

	public void Dispose()
	{
	}
}
