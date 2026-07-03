using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Analytics.Application.Handlers.Metrics;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Contracts.Analytics;
using Domain.CommonEntities;
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
) : LrtNamedObjectBase(
    jobRepository,
    unitOfWork,
    publisher,
    logger)
{
    public const string LrtSystemName = nameof(MetricCalculationLrt);
    public override Type InputType => typeof(MetricCalculationInputState);
    public override Type StateType => typeof(MetricCalculationState);
    public override string SystemName => LrtSystemName;
    public override string NameLocalizationKey => "metric_calculation_lrt_name";
    public override string DescriptionLocalizationKey => "metric_calculation_lrt_description";
    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Analytics;

    protected override async Task DoWork()
    {
        var state = await GetStateAsync<MetricCalculationInputState>()
                    ?? throw new InvalidOperationException($"'{InputType.Name}' state is null");

        await Publisher.Publish(
            new MetricCalculationStatusUpdatedEvent
            {
                MetricId = state.MetricId,
                JobStatus = Job.Status.ToString()
            });

        await UnitOfWork.SaveChangesAsync(CancellationToken);

        await sender.Send(new CalculateMetricCommand(state.MetricId), CancellationToken);
    }

    protected override async Task SucceedJobAsync()
    {
        await base.SucceedJobAsync();
        var state = await GetStateAsync<MetricCalculationState>()
                    ?? throw new InvalidOperationException("State is null");

        await Publisher.Publish(
            new MetricCalculationStatusUpdatedEvent
            {
                MetricId = state.MetricId,
                JobStatus = Job.Status.ToString()
            });

        await UnitOfWork.SaveChangesAsync(CancellationToken);
    }
}