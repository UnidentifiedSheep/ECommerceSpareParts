using Application.Common.Exceptions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Handlers.Jobs;

public record GetJobStateQuery(Guid JobId) : IQuery<GetJobStateResult>;

public record GetJobStateResult(string State, bool InputInitialized, string? Output);

public class GetJobStateHandler(IReadRepository<Job, Guid> repository)
	: IQueryHandler<GetJobStateQuery, GetJobStateResult>
{
	public async Task<GetJobStateResult> Handle(GetJobStateQuery request, CancellationToken cancellationToken)
	{
		var result = await repository
			.Query
			.Where(x => x.Id == request.JobId)
			.Select(x => new GetJobStateResult(x.State, x.InputInitialized, x.Output))
			.FirstOrDefaultAsync(cancellationToken) ?? throw new JobNotFoundException(request.JobId);

		return result;
	}
}
