using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Entities.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Attributes;
using Contracts.Analytics;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Lrts.MetricCalculation;

public class MetricCalculationLrt(
    IRepository<Job, Guid> jobRepository,
    IMetricCalculatorFactory calculatorFactory,
    IMetricRepository metricRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    ILogger<MetricCalculationLrt> logger
) : LrtBase<MetricCalculationInputState, MetricCalculationState>(
    jobRepository,
    unitOfWork,
    publisher,
    transactionService,
    logger)
{
    public const string LrtSystemName = nameof(MetricCalculationLrt);
    public override string SystemName => LrtSystemName;
    public override string NameLocalizationKey => "metric_calculation_lrt_name";
    public override string DescriptionLocalizationKey => "metric_calculation_lrt_description";

    protected override async Task DoWork()
    {
        await Publisher.Publish(
            new MetricCalculationStatusUpdatedEvent
            {
                MetricId = State.MetricId,
                JobStatus = Job.Status.ToString()
            });

        await UnitOfWork.SaveChangesAsync(CancellationToken);

        await TransactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                var metric = await metricRepository.GetById(
                    State.MetricId,
                    cancellationToken) ?? throw new MetricNotFoundException(State.MetricId);
                var calculator = calculatorFactory.GetCalculator(metric.GetType());

                await calculator.CalculateMetric(metric, cancellationToken);
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);
            },
            CancellationToken);
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
