using Security.Core.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Notifications;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Main.Enums.Auth;
using MediatR;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;

namespace Main.Application.Handlers.Auth.EmailVerification;

[Transactional]
[AutoSave]
public record RequestEmailVerificationCommand(Guid UserId, string Email) : ICommand;

public class RequestEmailVerificationHandler(
	IRepository<UserEmail, string> repository,
	IJsonSigner jsonSigner,
	INotificationService notificationService,
	IVerificationPayloadProvider verificationPayloadProvider,
	IContextualLocalizer localizer,
	IAppLinkProvider appLinkProvider) : ICommandHandler<RequestEmailVerificationCommand>
{
	public async Task<Unit> Handle(
		RequestEmailVerificationCommand request,
		CancellationToken cancellationToken)
	{
		var normalizedEmail = Email.ToNormalized(request.Email);
		var criteria = Criteria<UserEmail>.New().Where(x => x.Email == normalizedEmail).Track(false).Build();
		var userMail = await repository.FirstOrDefaultAsync(criteria, cancellationToken);

		if (userMail == null || userMail.UserId != request.UserId)
			throw new UserEmailNotFoundException(normalizedEmail);

		if (userMail.Confirmed)
			return Unit.Value;

		var signed = jsonSigner.Sign(
			await verificationPayloadProvider.GetPayload(
				request.UserId,
				VerificationType.EmailVerification,
				normalizedEmail));

		var verificationUrl = await appLinkProvider.CreateEmailVerificationUrlAsync(signed, cancellationToken);

		await notificationService.QueueAsync(
			request.UserId,
			new EmailVerificationNotification(
				new EmailVerificationNotificationData(localizer, verificationUrl.AbsoluteUri)),
			[new EmailRecipient(normalizedEmail)],
			cancellationToken);

		return Unit.Value;
	}
}
