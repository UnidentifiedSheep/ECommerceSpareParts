using Abstractions.Models.Repository;
using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Abstractions.Exceptions.MetricCalculationJobs;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Application.Common.Interfaces;
using Mapster;

namespace Analytics.Application.Handlers.CalculationJob.GetCalculationJob;

public record GetCalculationJobQuery(Guid RequestId) : IQuery<GetCalculationJobResult>;
public record GetCalculationJobResult(CalculationJobDto CalculationJob);

public class GetCalculationJobHandler(
    IMetricCalculationJobRepository repository)
    : IQueryHandler<GetCalculationJobQuery, GetCalculationJobResult>
{
    public async Task<GetCalculationJobResult> Handle(GetCalculationJobQuery request, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<MetricCalculationJob, Guid>()
        {
            Data = request.RequestId
        };
        var job = await repository.GetCalculationJob(queryOptions, cancellationToken) ??
                  throw new CalculationJobNotFoundException(request.RequestId);

        return new GetCalculationJobResult(job.Adapt<CalculationJobDto>());
    }
}