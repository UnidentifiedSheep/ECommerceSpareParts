using System.Globalization;

namespace Main.Application.Document;

public static class DocumentCulture
{
	public static bool TryGetSupported(string? name, out CultureInfo culture)
	{
		culture = CultureInfo.InvariantCulture;
		if (string.IsNullOrWhiteSpace(name)) return false;

		try
		{
			culture = CultureInfo.GetCultureInfo(name);
		}
		catch (CultureNotFoundException)
		{
			return false;
		}

		return culture.TwoLetterISOLanguageName is "en" or "ru" or "tr";
	}

	public static CultureInfo GetRequired(string? name)
		=> TryGetSupported(name, out var culture)
			? culture
			: throw new InvalidOperationException($"Document culture '{name}' is not supported.");
}
