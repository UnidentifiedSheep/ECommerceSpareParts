using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Analytics.Abstractions.Exceptions.MetricCalculationJobs;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Analytics.Enums;
using Application.Common.Interfaces;
using Attributes;

namespace Analytics.Application.Handlers.CalculationJob.UpdateCalculationJob;

[Transactional]
public record UpdateCalculationJobCommand(
    Guid RequestId, 
    CalculationStatus Status,
    Guid? MetricId) : ICommand<UpdateCalculationJobResult>;
public record UpdateCalculationJobResult(MetricCalculationJob CalculationJob);

public class UpdateCalculationJobHandler(
    IMetricCalculationJobRepository jobRepository,
    IUnitOfWork unitOfWork)
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

        if (job.MetricId != null && request.MetricId != null)
            throw new CalculationJobMetricIdUpdateException();

        job.MetricId = request.MetricId;
        job.Status = request.Status;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UpdateCalculationJobResult(job);
    }
}