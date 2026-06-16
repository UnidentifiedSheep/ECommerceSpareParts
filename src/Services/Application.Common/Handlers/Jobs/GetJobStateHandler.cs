using Application.Common.Exceptions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Handlers.Jobs;

public record GetJobStateQuery(Guid JobId) : IQuery<GetJobStateResult>;
public record GetJobStateResult(string State);

public class GetJobStateHandler(
    IReadRepository<Job, Guid> repository) : IQueryHandler<GetJobStateQuery, GetJobStateResult>
{
    public async Task<GetJobStateResult> Handle(GetJobStateQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.Query
                         .Where(x => x.Id == request.JobId)
                         .Select(x => x.State)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new JobNotFoundException(request.JobId);

        return new GetJobStateResult(result);
    }
}