using Abstractions.Models.Repository;
using Analytics.Abstractions.Exceptions.MetricCalculationJobs;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Analytics.Enums;
using Application.Common.Interfaces;
using Attributes;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.CalculationJob.UpdateCalculationJob;

[AutoSave]
[Transactional]
public record UpdateCalculationJobCommand(
    Guid RequestId, 
    CalculationStatus Status,
    Guid? MetricId,
    string? ErrorMessageKey) : ICommand<UpdateCalculationJobResult>;
public record UpdateCalculationJobResult(MetricCalculationJob CalculationJob);

public class UpdateCalculationJobHandler(
    IMetricCalculationJobRepository jobRepository,
    IScopedStringLocalizer localizer)
    : ICommandHandler<UpdateCalculationJobCommand, UpdateCalculationJobResult>
{
    public async Task<UpdateCalculationJobResult> Handle(UpdateCalculationJobCommand request, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<MetricCalculationJob, Guid>()
            {
                Data = request.RequestId
            }.WithTracking();

        var job = await jobRepository.GetCalculationJob(queryOptions, cancellationToken)
                  ?? throw new CalculationJobNotFoundException(request.RequestId);

        if (job.MetricId != null && request.MetricId != null && job.MetricId != request.MetricId)
            throw new CalculationJobMetricIdUpdateException();

        job.MetricId = request.MetricId;
        job.Status = request.Status;
        if (request.ErrorMessageKey != null)
            job.ErrorMessage = localizer[request.ErrorMessageKey];

        return new UpdateCalculationJobResult(job);
    }
}