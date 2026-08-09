using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Analytics.Application.NamedObjects.Analyzers;
using Analytics.Application.NamedObjects.Analyzers.Markup;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Attributes;
using Contracts.Analytics;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Lrts.MarkupCalculation;

public class MarkupCalculationLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    INamedObjectRegistry<MarkupAnalyzerNamedObjectBase> registry,
    IPublishEndpoint publisher,
    IDomainEventExecutor domainEventExecutor,
    ILogger<MarkupCalculationLrt> logger
) : LrtBase<MarkupCalculationInputState, MarkupCalculationState>(
    jobRepository,
    unitOfWork,
    publisher,
    domainEventExecutor,
    logger)
{
    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Analytics;
    public override string SystemName => nameof(MarkupCalculationLrt);
    public override string NameLocalizationKey => "markup_calculation_lrt_name";
    public override string DescriptionLocalizationKey => "markup_calculation_lrt_description";

    protected override async Task DoWork()
    {
        var analyzer = registry.GetBySystemName(MarkupRangeAnalyzer.AnalyzerSystemName);
        var result = await analyzer.AnalyzeAsync(
            new MarkupAnalyzerInput
            {
                StartDate = State.RangeStart,
                EndDate = State.RangeEnd
            },
            CancellationToken);

        var ranges = result.Select(x => new MarkupRangeItem
            {
                Count = x.Count,
                FromCost = x.FromCost,
                MeanMarkup = x.MeanMarkup,
                StdDevMarkup = x.StdDevMarkup,
                ToCost = x.ToCost
            })
            .ToList();

        await ExecuteWithDomainEventsTransactionAsync(
            TransactionalAttribute.ReadCommited(20, 2),
            async () =>
            {
                await Publisher.Publish(
                    new MarkupAnalyzedEvent
                    {
                        Ranges = ranges
                    });

                await UnitOfWork.SaveChangesAsync(CancellationToken);
            });
    }
}
