using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using Security.Core.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Validators;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.Auth.ChangePassword;

[AutoSave]
[Transactional]
public record ChangePasswordCommand(Guid UserId, string PreviousPassword, string NewPassword) : ICommand;

public class ChangePasswordHandler(IUserRepository userRepository, IPasswordManager passwordManager)
	: ICommandHandler<ChangePasswordCommand>
{
	public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
	{
		var user = await userRepository.FirstOrDefaultAsync(
				Criteria<User>.New()
					.Where(user => user.Id == request.UserId)
					.Track()
					.ForUpdate()
					.Build(),
				cancellationToken) ??
			throw new UserNotFoundException(request.UserId);

		if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.PreviousPassword))
			throw new WrongCredentialsException(null, request.PreviousPassword);

		user.SetPasswordHash(passwordManager.GetHashOfPassword(request.NewPassword));

		await userRepository.RevokeRefreshTokensAsync(
			user.Id,
			"Password changed",
			cancellationToken);
		return Unit.Value;
	}
}
