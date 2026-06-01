using Analytics.Entities;
using Analytics.Entities.Exceptions.MetricCalculationJobs;
using Analytics.Enums;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Metrics;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.CalculationJob.UpdateCalculationJob;

[Diagnostics(maxExecutionTimeMs: 200)]
[Transactional, AutoSave]
public record UpdateCalculationJobCommand(
    Guid RequestId,
    CalculationStatus Status,
    Guid? MetricId,
    string? ErrorMessageKey) : ICommand<UpdateCalculationJobResult>;

public record UpdateCalculationJobResult(MetricCalculationJob CalculationJob);

public class UpdateCalculationJobHandler(
    IRepository<MetricCalculationJob, Guid> jobRepository,
    IScopedStringLocalizer localizer,
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<UpdateCalculationJobCommand, UpdateCalculationJobResult>
{
    public async Task<UpdateCalculationJobResult> Handle(
        UpdateCalculationJobCommand request,
        CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetById(request.RequestId, cancellationToken)
                  ?? throw new CalculationJobNotFoundException(request.RequestId);

        var errorMessage = request.ErrorMessageKey != null
            ? localizer[request.ErrorMessageKey]
            : null;

        try
        {
            UpdateJob(job, request.Status, request.MetricId, errorMessage);
        }
        catch (InvalidOperationException) when (
            request.MetricId.HasValue &&
            job.MetricId.HasValue &&
            request.MetricId.Value != job.MetricId.Value)
        {
            throw new CalculationJobMetricIdUpdateException();
        }
        
        integrationEventScope.Add(new MetricCalculationJobUpdatedEvent
        {
            MetricId = request.MetricId,
            RequestId = request.RequestId,
            CalculationStatus = job.Status.ToString(),
            ErrorMessage = errorMessage
        });

        return new UpdateCalculationJobResult(job);
    }

    private static void UpdateJob(
        MetricCalculationJob job,
        CalculationStatus status,
        Guid? metricId,
        string? errorMessage)
    {
        switch (status)
        {
            case CalculationStatus.AwaitingWorker:
                throw new InvalidOperationException("Calculation job can not be returned to awaiting worker status.");
            case CalculationStatus.Calculating:
                if (!metricId.HasValue)
                    throw new InvalidOperationException("Metric id is required to start calculation job.");
                job.Start(metricId.Value);
                break;
            case CalculationStatus.Succeeded:
                if (!metricId.HasValue)
                    throw new InvalidOperationException("Metric id is required to complete calculation job.");
                job.Succeed(metricId.Value);
                break;
            case CalculationStatus.Failed:
                job.Fail(metricId, errorMessage);
                break;
            case CalculationStatus.Cancelled:
                job.Cancel(errorMessage);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
}