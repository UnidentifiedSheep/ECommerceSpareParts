namespace Application.Common.Interfaces.Links;

public static class AppLinkTemplates
{
	public static bool IsValid(string? template, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(template) || template.StartsWith('/') || template.StartsWith('\\'))
			return false;

		var placeholder = $"{{{parameterName}}}";
		if (!template.Contains(placeholder, StringComparison.Ordinal))
			return false;

		var rendered = template.Replace(
			placeholder,
			"value",
			StringComparison.Ordinal);
		return rendered.IndexOfAny(['{', '}']) < 0 &&
			!Uri.TryCreate(
				rendered,
				UriKind.Absolute,
				out _) &&
			Uri.TryCreate(
				rendered,
				UriKind.Relative,
				out _);
	}
}
