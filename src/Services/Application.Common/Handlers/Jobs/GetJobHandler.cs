using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Handlers.Jobs;

public record GetJobQuery(Guid JobId) : IQuery<GetJobResult>;

public record GetJobResult(JobDto Job);

public class GetJobHandler(IReadRepository<Job, Guid> repository, IProjectionProvider<Job, JobDto> projection)
	: IQueryHandler<GetJobQuery, GetJobResult>
{
	public async Task<GetJobResult> Handle(GetJobQuery request, CancellationToken cancellationToken)
	{
		var job = await repository
			.Query
			.Where(x => x.Id == request.JobId)
			.Project(projection)
			.FirstOrDefaultAsync(cancellationToken) ?? throw new JobNotFoundException(request.JobId);

		return new GetJobResult(job);
	}
}
