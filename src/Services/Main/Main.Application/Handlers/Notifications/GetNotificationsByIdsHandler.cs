using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Microsoft.EntityFrameworkCore;
using Notification.Core.Entities;

namespace Main.Application.Handlers.Notifications;

public record GetNotificationsByIdsQuery : IQuery<GetNotificationsByIdsResult>
{
	public IReadOnlyList<int> Ids { get; }
	public Guid UserId { get; }

	public GetNotificationsByIdsQuery(IEnumerable<int> ids, Guid userId)
	{
		Ids = ids.Distinct().ToList();
		UserId = userId;
	}
}

public record GetNotificationsByIdsResult(
	IReadOnlyDictionary<int, InAppNotificationDto> Notifications);

public class GetNotificationsByIdsHandler(
	IReadRepository<InAppNotification, int> repository,
	IProjectionProvider<InAppNotification, InAppNotificationDto> projection
	) : IQueryHandler<GetNotificationsByIdsQuery, GetNotificationsByIdsResult>
{
	public async Task<GetNotificationsByIdsResult> Handle(
		GetNotificationsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetNotificationsByIdsResult(new Dictionary<int, InAppNotificationDto>());

		var result = await repository.Query
			.Where(x => x.UserId == request.UserId)
			.Where(x => request.Ids.Contains(x.Id))
			.Project(projection)
			.ToDictionaryAsync(x => x.Id, cancellationToken);

		return new GetNotificationsByIdsResult(result);
	}
}
