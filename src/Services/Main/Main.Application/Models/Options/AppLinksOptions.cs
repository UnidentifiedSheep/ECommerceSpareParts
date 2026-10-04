namespace Main.Application.Models.Options;

public sealed class AppLinksOptions
{
	public const string SectionName = "AppLinks";

	public string PasswordReset { get; set; } = "reset?token={token}";

	public string EmailVerification { get; set; } = "verify-email?token={token}";

	public string Document { get; set; } = "documents/{requestId}";

	public bool IsValid() =>
		IsValidTemplate(PasswordReset, "token") &&
		IsValidTemplate(EmailVerification, "token") &&
		IsValidTemplate(Document, "requestId");

	private static bool IsValidTemplate(string? template, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(template) || template.StartsWith('/') || template.StartsWith('\\'))
			return false;

		var placeholder = $"{{{parameterName}}}";
		if (!template.Contains(placeholder, StringComparison.Ordinal))
			return false;

		var rendered = template.Replace(placeholder, "value", StringComparison.Ordinal);
		return rendered.IndexOfAny(['{', '}']) < 0 &&
		       !Uri.TryCreate(rendered, UriKind.Absolute, out _) &&
		       Uri.TryCreate(rendered, UriKind.Relative, out _);
	}
}
