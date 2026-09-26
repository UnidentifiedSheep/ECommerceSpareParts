using Application.Common.Interfaces.Cqrs;
using Main.Application.Notifications;
using MediatR;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;

namespace Main.Application.Handlers.Notifications;

public record SendNotificationCommand(Guid UserId, string Text) : ICommand<SendNotificationResult>;
public record SendNotificationResult(bool Succeeded);

public class SendNotificationHandler(
	INotificationService notificationService
	) : ICommandHandler<SendNotificationCommand, SendNotificationResult>
{
	public async Task<SendNotificationResult> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
	{
		var res = await notificationService.SendAsync(
			new InAppRecipient(request.UserId),
			new SimpleTextNotification(request.Text),
			cancellationToken);

		return new SendNotificationResult(res.Succeeded);
	}
}
