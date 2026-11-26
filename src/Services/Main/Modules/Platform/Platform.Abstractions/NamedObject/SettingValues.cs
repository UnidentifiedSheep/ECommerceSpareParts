using System.Diagnostics.CodeAnalysis;

namespace Application.Common.NamedObject;

public static class SettingValues
{
	public static TOut Patch<TIn, TOut>(TIn? input, TOut current, Func<TIn, TOut> map) where TIn : class
		=> input is null ? current : map(input);

	public static string Text(string value) => value.Trim();

	public static string? TextOrNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

	public static string Url(string value)
		=> TryParseUrl(value, out var url)
			? url
			: throw new ArgumentException($"'{value}' is not an absolute http or https URL.", nameof(value));

	public static bool TryParseUrl(string? value, [NotNullWhen(true)] out string? url)
	{
		url = null;
		if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(
				value.Trim(),
				UriKind.Absolute,
				out var uri) || uri.Scheme is not ("http" or "https"))
			return false;

		url = uri.AbsoluteUri.TrimEnd('/');
		return true;
	}
}
