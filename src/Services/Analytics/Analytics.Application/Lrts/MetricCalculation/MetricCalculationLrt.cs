using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Analytics.Application.Handlers.Metrics;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Contracts.Analytics;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Lrts.MetricCalculation;

public class MetricCalculationLrt(
    IRepository<Job, Guid> jobRepository,
    ISender sender,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger<MetricCalculationLrt> logger
) : LrtBase<MetricCalculationInputState, MetricCalculationState>(
    jobRepository,
    unitOfWork,
    publisher,
    logger)
{
    public const string LrtSystemName = nameof(MetricCalculationLrt);
    public override string SystemName => LrtSystemName;
    public override string NameLocalizationKey => "metric_calculation_lrt_name";
    public override string DescriptionLocalizationKey => "metric_calculation_lrt_description";
    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Analytics;

    protected override async Task DoWork()
    {
        await Publisher.Publish(
            new MetricCalculationStatusUpdatedEvent
            {
                MetricId = State.MetricId,
                JobStatus = Job.Status.ToString()
            });

        await UnitOfWork.SaveChangesAsync(CancellationToken);

        await sender.Send(new CalculateMetricCommand(State.MetricId), CancellationToken);
    }

    protected override async Task SucceedJobAsync()
    {
        await base.SucceedJobAsync();
        await Publisher.Publish(
            new MetricCalculationStatusUpdatedEvent
            {
                MetricId = State.MetricId,
                JobStatus = Job.Status.ToString()
            });

        await UnitOfWork.SaveChangesAsync(CancellationToken);
    }
}
