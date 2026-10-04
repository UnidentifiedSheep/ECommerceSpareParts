using Security.Core.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Notifications;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Entities.User;
using Main.Enums.Auth;
using MediatR;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;

namespace Main.Application.Handlers.Auth.PasswordRecovery.SendEmailRecovery;

[Transactional]
[AutoSave]
public record SendEmailRecoveryCommand(string Email) : ICommand;

public class SendEmailRecoveryHandler(
	IJsonSigner jsonSigner,
	IUserRepository userRepository,
	INotificationService notificationService,
	IContextualLocalizer localizer,
	IResetPayloadProvider payloadProvider,
	IAppLinkProvider appLinkProvider) : ICommandHandler<SendEmailRecoveryCommand>
{
	public async Task<Unit> Handle(SendEmailRecoveryCommand request, CancellationToken cancellationToken)
	{
		var user = await userRepository.GetUserByPrimaryEmailAsync(
			request.Email,
			Criteria<User>.New().Track(false).Build(),
			cancellationToken);

		if (user == null)
			return Unit.Value;

		var signed = jsonSigner.Sign(await payloadProvider.GetPayload(user.Id, ResetType.PasswordReset));
		var resetUrl = await appLinkProvider.CreatePasswordResetUrlAsync(signed, cancellationToken);

		await notificationService.QueueAsync(
			user.Id,
			new PasswordResetNotification(
				new PasswordResetNotificationData(localizer, resetUrl.AbsoluteUri)),
			[new EmailRecipient(request.Email)],
			cancellationToken);

		return Unit.Value;
	}
}
