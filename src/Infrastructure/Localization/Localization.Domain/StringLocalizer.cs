using Localization.Abstractions;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Localization.Domain;

public class StringLocalizer : IStringLocalizer
{
	private readonly Dictionary<Locale, ILocalizerContainer> _containers = [];

	public StringLocalizer(IEnumerable<ILocalizerContainer> containers)
	{
		foreach (var container in containers)
			_containers[container.Locale] = container;
	}

	public string Get(string key, Locale locale)
	{
		if (!_containers.TryGetValue(locale, out var container))
			throw new InvalidOperationException($"Locale '{locale}' not found");
		if (!container.KetMessages.TryGetValue(key, out var value))
			throw new InvalidOperationException($"Unable to find value for {key} in {locale} locale");

		return value;
	}

	public string Get(
		string key,
		Locale locale,
		params object[] arguments)
	{
		var template = Get(key, locale);
		LocalizedMessageFormatter.TryFormat(
			template,
			arguments,
			out var value);
		return value;
	}

	public bool TryGet(
		string key,
		Locale locale,
		out string? value)
	{
		value = null;
		if (!_containers.TryGetValue(locale, out var container))
			return false;
		return container.KetMessages.TryGetValue(key, out value);
	}

	public bool TryGet(
		string key,
		Locale locale,
		out string? value,
		params object[] arguments)
	{
		if (!TryGet(
				key,
				locale,
				out value) || value == null)
			return false;

		return LocalizedMessageFormatter.TryFormat(
			value,
			arguments,
			out value);
	}

	public bool IsSupported(Locale locale) => _containers.ContainsKey(locale);
}
