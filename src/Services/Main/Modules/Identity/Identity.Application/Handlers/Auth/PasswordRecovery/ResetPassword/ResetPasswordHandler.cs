using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Enums.Auth;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Modules.Identity.Application.Interfaces.Persistence;
using Main.Modules.Identity.Application.Interfaces.Services.PayloadProvider;
using Main.Modules.Identity.Application.Models.Auth;
using MediatR;
using Security.Core.Interfaces;

namespace Main.Modules.Identity.Application.Handlers.Auth.PasswordRecovery.ResetPassword;

[Transactional]
[AutoSave]
public record ResetPasswordCommand(string Token, string NewPassword) : ICommand;

public class ResetPasswordHandler(
	IUserRepository userRepository,
	IPasswordManager pwdManager,
	IResetPayloadProvider resetPayloadProvider,
	IJsonSigner jsonSigner) : ICommandHandler<ResetPasswordCommand>
{
	public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
	{
		var token = Uri.UnescapeDataString(request.Token);
		if (!jsonSigner.VerifyJson<ResetPayload>(token, out var payload) ||
			!await resetPayloadProvider.IsTokenValid(payload.Id) || payload.Type != ResetType.PasswordReset)
			throw new ResetTokenExpiredException();

		var user = await userRepository.FirstOrDefaultAsync(
				Criteria<User>.New()
					.Where(user => user.Id == payload.UserId)
					.Track()
					.ForUpdate()
					.Build(),
				cancellationToken) ??
			throw new UserNotFoundException(payload.UserId);
		var pwdHash = pwdManager.GetHashOfPassword(request.NewPassword);

		user.SetPasswordHash(pwdHash);
		await userRepository.RevokeRefreshTokensAsync(
			user.Id,
			"Password changed",
			cancellationToken);
		return Unit.Value;
	}
}
