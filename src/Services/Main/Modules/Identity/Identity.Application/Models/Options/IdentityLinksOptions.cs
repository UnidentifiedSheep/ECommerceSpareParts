using Application.Common.Interfaces.Links;

namespace Main.Modules.Identity.Application.Models.Options;

public sealed class IdentityLinksOptions
{
	public const string SectionName = "AppLinks";

	public string PasswordReset { get; set; } = "reset?token={token}";

	public string EmailVerification { get; set; } = "verify-email?token={token}";

	public bool IsValid()
		=> AppLinkTemplates.IsValid(PasswordReset, "token") &&
			AppLinkTemplates.IsValid(EmailVerification, "token");
}
