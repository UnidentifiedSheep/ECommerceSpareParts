using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models.Auth;
using Main.Entities.Exceptions;
using Main.Enums.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.PasswordRecovery.ResetPassword;

[Transactional, AutoSave]
public record ResetPasswordCommand(
    string Token,
    string NewPassword) : ICommand;

public class ResetPasswordHandler(
    IUserRepository userRepository,
    IPasswordManager pwdManager,
    IJsonSigner jsonSigner) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Unit> Handle(
        ResetPasswordCommand request, 
        CancellationToken cancellationToken)
    {
        var token = Uri.UnescapeDataString(request.Token);
        if (!jsonSigner.VerifyJson<ResetPayload>(token, out var payload) || 
            payload == null || 
            payload.Expires <= DateTime.UtcNow ||
            payload.Type != ResetType.PasswordReset)
            throw new ResetTokenExpiredException();

        var user = await userRepository.GetById(payload.UserId, cancellationToken)
                   ?? throw new UserNotFoundException(payload.UserId);
        var pwdHash = pwdManager.GetHashOfPassword(request.NewPassword);
        
        user.SetPasswordHash(pwdHash);
        return Unit.Value;
    }
}