using Abstractions.Models;
using Application.Common.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Handlers.Jobs.GetJobs;

public record GetJobsQuery(
	Pagination Pagination,
	IEnumerable<string> SystemNames,
	IEnumerable<JobStatus> Statuses,
	string[] SortBy) : IQuery<GetJobsResult>;

public record GetJobsResult(IReadOnlyList<JobDto> Jobs);

public class GetJobsHandler(
	IReadRepository<Job, Guid> repository,
	IProjectionProvider<Job, JobDto> projection) : IQueryHandler<GetJobsQuery, GetJobsResult>
{
	public async Task<GetJobsResult> Handle(GetJobsQuery request, CancellationToken cancellationToken)
	{
		var query = repository.Query;

		if (request.SystemNames.Any())
			query = query.Where(x => request.SystemNames.Contains(x.SystemName));

		if (request.Statuses.Any())
			query = query.Where(x => request.Statuses.Contains(x.Status));

		var result = await query
			.SortBy(request.SortBy)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetJobsResult(result);
	}
}
