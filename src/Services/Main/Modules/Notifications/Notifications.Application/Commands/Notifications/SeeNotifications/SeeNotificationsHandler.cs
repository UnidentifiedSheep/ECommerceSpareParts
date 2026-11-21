using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Notification;
using MediatR;

namespace Main.Modules.Notifications.Application.Commands.Notifications.SeeNotifications;

[Transactional]
[AutoSave]
public record SeeNotificationsCommand : ICommand
{

	public SeeNotificationsCommand(Guid userId, IEnumerable<int> ids)
	{
		UserId = userId;
		Ids = ids.Distinct().ToList();
	}
	public Guid UserId { get; }
	public IReadOnlyList<int> Ids { get; }
}

public class SeeNotificationsHandler(IRepository<InAppNotification, int> repository)
	: ICommandHandler<SeeNotificationsCommand>
{
	public async Task<Unit> Handle(SeeNotificationsCommand request, CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0) return Unit.Value;

		var notifications = await repository.ListAsync(
			Criteria<InAppNotification>.New()
				.Where(x => x.UserId == request.UserId)
				.Where(x => request.Ids.Contains(x.Id))
				.Track()
				.Build(),
			cancellationToken);

		foreach (var notification in notifications)
		{
			if (notification.IsSeen) continue;
			notification.See();
		}

		return Unit.Value;
	}
}
