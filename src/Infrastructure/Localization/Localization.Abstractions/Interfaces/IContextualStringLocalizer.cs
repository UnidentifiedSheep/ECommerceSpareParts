using Localization.Abstractions.Models;

namespace Localization.Abstractions.Interfaces;

public interface IContextualStringLocalizer
{
	Locale Locale { get; }

	string this[string key] { get; }

	string Get(string key);

	string Get(string key, params object[] arguments);

	bool TryGet(string key, out string? value);

	bool TryGet(
		string key,
		out string? value,
		params object[] arguments);

	string? GetOrDefault(string key);

	string? GetOrDefault(string key, params object[] arguments);
}
