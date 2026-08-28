using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Application.Models.Auth;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Main.Enums.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.EmailVerification;

[Transactional, AutoSave]
public record VerifyEmailCommand(string Token) : ICommand;

public class VerifyEmailHandler(
    IJsonSigner signer,
    IRepository<UserEmail, string> repository,
    IVerificationPayloadProvider payloadProvider) : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var payload = GetPayload(request.Token);
        if (payload.Type != VerificationType.EmailVerification)
            throw new EmailVerificationTokenExpiredException();

        var normalizedEmail = Email.ToNormalized(payload.DataToVerify);
        var email = await repository.FirstOrDefaultAsync(
            Criteria<UserEmail>.New()
                .Where(x => x.Email == normalizedEmail)
                .Track()
                .Build(),
            cancellationToken);

        if (email == null || email.UserId != payload.UserId)
            throw new UserEmailNotFoundException(normalizedEmail);

        if (!await payloadProvider.TryConsumeToken(payload.Id))
            throw new EmailVerificationTokenExpiredException();

        email.Confirm();
        return Unit.Value;
    }

    private VerificationPayload GetPayload(string token)
    {
        var decodedToken = Uri.UnescapeDataString(token);
        if (signer.VerifyJson<VerificationPayload>(decodedToken, out var payload))
            return payload;

        throw new EmailVerificationTokenExpiredException();
    }
}
