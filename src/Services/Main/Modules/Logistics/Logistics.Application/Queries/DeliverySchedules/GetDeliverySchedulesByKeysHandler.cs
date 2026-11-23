using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Logistics.Application.Queries.DeliverySchedules;

public record GetDeliverySchedulesByKeysQuery : IQuery<GetDeliverySchedulesByKeysResult>
{
	public GetDeliverySchedulesByKeysQuery(IEnumerable<DeliveryScheduleKey> keys)
		=> Keys = keys.Select(key => key with { StorageCode = key.StorageCode.Trim() }).ToHashSet();

	public IReadOnlySet<DeliveryScheduleKey> Keys { get; }
}

public record GetDeliverySchedulesByKeysResult(IReadOnlyDictionary<DeliveryScheduleKey, DeliveryScheduleDto> Schedules);

public class GetDeliverySchedulesByKeysHandler(
	IReadRepository<DeliverySchedule, DeliveryScheduleKey> repository,
	IProjectionProvider<DeliverySchedule, DeliveryScheduleDto> projection)
	: IQueryHandler<GetDeliverySchedulesByKeysQuery, GetDeliverySchedulesByKeysResult>
{
	public async Task<GetDeliverySchedulesByKeysResult> Handle(
		GetDeliverySchedulesByKeysQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Keys.Count == 0)
			return new GetDeliverySchedulesByKeysResult(new Dictionary<DeliveryScheduleKey, DeliveryScheduleDto>());

		var schedules = await repository
			.QueryByIds(request.Keys)
			.Project(projection)
			.ToDictionaryAsync(x => new DeliveryScheduleKey(x.Supplier, x.StorageCode), cancellationToken);

		return new GetDeliverySchedulesByKeysResult(schedules);
	}
}
