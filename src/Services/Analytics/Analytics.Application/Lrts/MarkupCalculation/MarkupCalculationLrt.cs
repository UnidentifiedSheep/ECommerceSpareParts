using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Analytics.Application.NamedObjects.Analyzers;
using Analytics.Application.NamedObjects.Analyzers.Markup;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Attributes;
using Contracts.Analytics;
using Domain.CommonEntities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Lrts.MarkupCalculation;

public class MarkupCalculationLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    INamedObjectRegistry<MarkupAnalyzerNamedObjectBase> registry,
    IPublishEndpoint publisher,
    ILogger<MarkupCalculationLrt> logger
) : LrtNamedObjectBase(
    jobRepository,
    unitOfWork,
    publisher,
    logger)
{
    protected override IServiceDefinition ServiceDefinition => ServicesDefinitions.Analytics;
    public override Type InputType => typeof(MarkupCalculationInputState);
    public override Type StateType => typeof(MarkupCalculationState);
    public override string SystemName => nameof(MarkupCalculationLrt);
    public override string NameLocalizationKey => "markup_calculation_lrt_name";
    public override string DescriptionLocalizationKey => "markup_calculation_lrt_description";

    protected override async Task DoWork()
    {
        var state = await GetStateAsync<MarkupCalculationState>()
                    ?? throw new InvalidOperationException($"'{InputType.Name}' state is null");

        var analyzer = registry.GetBySystemName(MarkupRangeAnalyzer.AnalyzerSystemName);
        var result = await analyzer.AnalyzeAsync(
            new MarkupAnalyzerInput
            {
                StartDate = state.RangeStart,
                EndDate = state.RangeEnd
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

        await UnitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(20, 2),
            async () =>
            {
                await Publisher.Publish(
                    new MarkupAnalyzedEvent
                    {
                        Ranges = ranges
                    });

                await UnitOfWork.SaveChangesAsync(CancellationToken);
            },
            CancellationToken);
    }
}