using Analytics.Application.Dtos.CalculationJob;
using Analytics.Application.Handlers.Projections;
using Analytics.Entities;
using Analytics.Entities.Exceptions.MetricCalculationJobs;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Handlers.CalculationJob.GetCalculationJob;

public record GetCalculationJobQuery(Guid RequestId) : IQuery<GetCalculationJobResult>;

public record GetCalculationJobResult(CalculationJobDto CalculationJob);

public class GetCalculationJobHandler(
    IReadRepository<MetricCalculationJob, Guid> repository)
    : IQueryHandler<GetCalculationJobQuery, GetCalculationJobResult>
{
    public async Task<GetCalculationJobResult> Handle(
        GetCalculationJobQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.Query
                         .Where(x => x.RequestId == request.RequestId)
                         .AsExpandable()
                         .Select(MetricCalculationJobProjection.ToDto)
                         .FirstOrDefaultAsync(cancellationToken) ??
                     throw new CalculationJobNotFoundException(request.RequestId);

        return new GetCalculationJobResult(result);
    }
}