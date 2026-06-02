using Abstractions.Interfaces.Mail;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Main.Application.EmailMessages;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Auth;
using Main.Entities.Setting;
using Main.Entities.User;
using MediatR;

namespace Main.Application.Handlers.Auth.PasswordRecovery.SendEmailRecovery;

[Transactional, AutoSave]
public record SendEmailRecoveryCommand(string Email) : ICommand;

public class SendEmailRecoveryHandler(
    IJsonSigner jsonSigner,
    IUserRepository userRepository,
    IMailingService mailingService,
    ISettingsService settingsService) : ICommandHandler<SendEmailRecoveryCommand>
{
    public async Task<Unit> Handle(
        SendEmailRecoveryCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await userRepository
            .GetUserByPrimaryEmailAsync(
                request.Email,
                Criteria<User>.New().Track(false).Build(),
                cancellationToken);

        if (user == null) return Unit.Value;
        
        var setting = (await settingsService
                .GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data;

        var signed = jsonSigner.Sign(new ResetPayload
        {
            UserId = user.Id,
            Email = request.Email.Trim().ToLowerInvariant(),
            Expires = DateTime.UtcNow + TimeSpan.FromMinutes(30)
        });

        var baseUri = new Uri(setting.AppServiceUrl.TrimEnd('/') + "/");
        var resetUrl = new Uri(baseUri, $"reset?token={Uri.EscapeDataString(signed)}");

        await mailingService.QueueToOutbox(
            new PasswordResetEmailMessage(request.Email, resetUrl),
            cancellationToken);

        return Unit.Value;
    }
}
