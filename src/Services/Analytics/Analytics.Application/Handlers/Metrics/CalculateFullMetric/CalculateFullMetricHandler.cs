using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Analytics.Abstractions.Exceptions.MetricCalculationJobs;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Application.Handlers.CalculationJob.UpdateCalculationJob;
using Analytics.Application.Handlers.Metrics.CalculateMetric;
using Analytics.Application.Handlers.Metrics.CreateMetric;
using Analytics.Entities;
using Analytics.Entities.Metrics;
using Analytics.Enums;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers.Metrics.CalculateFullMetric;

public record CalculateFullMetricCommand(
    Guid RequestId,
    string MetricSystemName, 
    string MetricPayload, 
    Guid CreatedBy) : ICommand;

public class CalculateFullMetricHandler(
    ISender sender,
    ILogger<CalculateFullMetricHandler> logger,
    IMetricCalculationJobRepository jobRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CalculateFullMetricCommand>
{
    public async Task<Unit> Handle(CalculateFullMetricCommand request, CancellationToken ct)
    {
        logger.LogInformation(
            "Starting full metric calculation. RequestId: {RequestId}, SystemName: {SystemName}",
            request.RequestId,
            request.MetricSystemName);

        unitOfWork.Context.SuppressAutoSave = true;
        
        var job = await GetAndValidateJob(request.RequestId, ct);
        
        var metric = await CreateMetricAndStartJob(request, job, ct);

        try
        {
            metric = await CalculateMetric(metric.Id, ct);
            
            await CompleteJobSuccessfully(job.RequestId, metric.Id, ct);

            logger.LogInformation(
                "Metric calculation completed successfully. RequestId: {RequestId}, MetricId: {MetricId}",
                job.RequestId,
                metric.Id);
        }
        catch (Exception e)
        {
            await FailJob(job.RequestId, metric, e, ct);
        }

        return Unit.Value;
    }
    
    private async Task<Metric> CreateMetricAndStartJob(
        CalculateFullMetricCommand request,
        MetricCalculationJob job,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Creating metric and starting job. RequestId: {RequestId}",
            job.RequestId);

        var metric = (await sender.Send(new CreateMetricCommand(
            request.MetricSystemName,
            request.MetricPayload,
            request.CreatedBy), ct)).Metric;

        logger.LogInformation(
            "Metric created. MetricId: {MetricId}, RequestId: {RequestId}",
            metric.Id,
            job.RequestId);

        await sender.Send(new UpdateCalculationJobCommand(
            job.RequestId,
            CalculationStatus.Calculating,
            metric.Id), ct);

        logger.LogInformation(
            "Job status updated to Calculating. RequestId: {RequestId}",
            job.RequestId);

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogDebug(
            "Initial changes saved. RequestId: {RequestId}",
            job.RequestId);

        return metric;
    }
    
    private async Task<Metric> CalculateMetric(Guid metricId, CancellationToken ct)
    {
        logger.LogInformation(
            "Starting metric calculation. MetricId: {MetricId}",
            metricId);

        var result = (await sender.Send(new CalculateMetricCommand(metricId), ct))
            .CalculatedMetric;

        logger.LogInformation(
            "Metric calculation finished. MetricId: {MetricId}",
            metricId);

        return result;
    }
    
    private async Task CompleteJobSuccessfully(
        Guid requestId,
        Guid metricId,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Marking job as succeeded. RequestId: {RequestId}, MetricId: {MetricId}",
            requestId,
            metricId);

        await sender.Send(new UpdateCalculationJobCommand(
            requestId,
            CalculationStatus.Succeeded,
            metricId), ct);

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogDebug(
            "Success state persisted. RequestId: {RequestId}",
            requestId);
    }

    private async Task FailJob(
        Guid requestId,
        Metric metric,
        Exception e,
        CancellationToken ct)
    {
        logger.LogError(e,
            "Metric calculation failed. RequestId: {RequestId}, MetricId: {MetricId}",
            requestId,
            metric.Id);

        await sender.Send(new UpdateCalculationJobCommand(
            requestId,
            CalculationStatus.Failed,
            metric.Id), ct);

        unitOfWork.Remove(metric);

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Failure state persisted and metric removed. RequestId: {RequestId}, MetricId: {MetricId}",
            requestId,
            metric.Id);

        throw new InvalidOperationException(
            $"Failed to calculate metric for request id: {requestId}. Error: {e.Message}", e);
    }

    private async Task<MetricCalculationJob> GetAndValidateJob(
        Guid requestId,
        CancellationToken ct)
    {
        logger.LogDebug(
            "Fetching calculation job. RequestId: {RequestId}",
            requestId);

        var queryOptions = new QueryOptions<MetricCalculationJob, Guid>()
        {
            Data = requestId,
        }.WithTracking();

        var job = await jobRepository.GetCalculationJob(queryOptions, ct)
            ?? throw new CalculationJobNotFoundException(requestId);

        if (job.Status == CalculationStatus.AwaitingWorker) return job;
        
        logger.LogWarning(
            "Invalid job status. RequestId: {RequestId}, Status: {Status}",
            requestId,
            job.Status);

        throw new InvalidOperationException("The calculation job is not awaiting worker.");
    }
}