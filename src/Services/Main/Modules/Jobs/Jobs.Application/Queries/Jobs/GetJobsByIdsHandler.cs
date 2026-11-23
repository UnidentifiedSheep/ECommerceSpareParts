using Application.Common.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Jobs.Application.Queries.Jobs;

public sealed record GetJobsByIdsQuery : IQuery<GetJobsByIdsResult>
{
	public GetJobsByIdsQuery(Guid id) : this([id]) { }

	public GetJobsByIdsQuery(IEnumerable<Guid> ids) => Ids = ids.Distinct().ToArray();

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetJobsByIdsResult(IReadOnlyDictionary<Guid, JobDto> Jobs);

public sealed class GetJobsByIdsHandler(
	IReadRepository<Job, Guid> repository,
	IProjectionProvider<Job, JobDto> projection)
	: IQueryHandler<GetJobsByIdsQuery, GetJobsByIdsResult>
{
	public async Task<GetJobsByIdsResult> Handle(
		GetJobsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetJobsByIdsResult(new Dictionary<Guid, JobDto>());

		var jobs = await repository.Query
			.Where(job => request.Ids.Contains(job.Id))
			.Project(projection)
			.ToDictionaryAsync(job => job.Id, cancellationToken);

		return new GetJobsByIdsResult(jobs);
	}
}
