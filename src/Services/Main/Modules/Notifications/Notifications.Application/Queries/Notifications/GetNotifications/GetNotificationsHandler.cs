using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Entities.Notification;
using Main.Modules.Notifications.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Notifications.Application.Queries.Notifications.GetNotifications;

public record GetNotificationsQuery(
	Guid UserId,
	Cursor<DateTime?> Cursor
) : IQuery<GetNotificationsResult>;

public record GetNotificationsResult(
	IReadOnlyList<InAppNotificationDto> Notifications);

public class GetNotificationsHandler(
	IReadRepository<InAppNotification, int> repository,
	IProjectionProvider<InAppNotification, InAppNotificationDto> projection
) : IQueryHandler<GetNotificationsQuery, GetNotificationsResult>
{
	public async Task<GetNotificationsResult> Handle(
		GetNotificationsQuery request,
		CancellationToken cancellationToken)
	{
		var result = await repository.Query
			.Where(x => x.UserId == request.UserId)
			.ApplyCursor(request.Cursor)
			.Project(projection)
			.ToListAsync(cancellationToken);

		return new GetNotificationsResult(result);
	}
}
