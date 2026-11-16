using Application.Common.Interfaces.Links;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Options;
using Microsoft.Extensions.Options;

namespace Main.Application.Services;

public sealed class AuthLinkProvider(
	IAppLinkBuilder linkBuilder,
	IOptions<IdentityLinksOptions> options) : IAuthLinkProvider
{
	public Task<Uri> CreatePasswordResetUrlAsync(string token, CancellationToken cancellationToken = default)
		=> linkBuilder.CreateAsync(
			options.Value.PasswordReset,
			new Dictionary<string, string>
			{
				["token"] = token
			},
			cancellationToken);

	public Task<Uri> CreateEmailVerificationUrlAsync(
		string token,
		CancellationToken cancellationToken = default)
		=> linkBuilder.CreateAsync(
			options.Value.EmailVerification,
			new Dictionary<string, string>
			{
				["token"] = token
			},
			cancellationToken);
}
