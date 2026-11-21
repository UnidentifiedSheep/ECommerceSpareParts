using Application.Common.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Jobs.Application.Queries.JobSchedules;

public sealed record GetSchedulesByIdsQuery : IQuery<GetSchedulesByIdsResult>
{
	public GetSchedulesByIdsQuery(Guid id) : this([id]) { }

	public GetSchedulesByIdsQuery(IEnumerable<Guid> ids) => Ids = ids.Distinct().ToArray();

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetSchedulesByIdsResult(IReadOnlyDictionary<Guid, JobScheduleDto> Schedules);

public sealed class GetSchedulesByIdsHandler(
	IReadRepository<JobSchedule, Guid> repository,
	IProjectionProvider<JobSchedule, JobScheduleDto> projection)
	: IQueryHandler<GetSchedulesByIdsQuery, GetSchedulesByIdsResult>
{
	public async Task<GetSchedulesByIdsResult> Handle(
		GetSchedulesByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetSchedulesByIdsResult(new Dictionary<Guid, JobScheduleDto>());

		var schedules = await repository.Query
			.Where(schedule => request.Ids.Contains(schedule.Id))
			.Project(projection)
			.ToDictionaryAsync(schedule => schedule.Id, cancellationToken);

		return new GetSchedulesByIdsResult(schedules);
	}
}
