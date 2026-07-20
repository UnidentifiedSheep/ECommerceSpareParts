using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Handlers.Jobs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Attributes;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Jobs;
using Pricing.Entities.Offers;
using Pricing.Entities.Settings;

namespace Pricing.Application.Lrts.InvalidateStalePriceOptions;

public class InvalidateStalePriceOptionsLrt(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger<InvalidateStalePriceOptionsLrt> logger,
    IReadRepository<ProductPriceOption, Guid> readRepository,
    IProductPriceOptionRepository productPriceOptionRepository,
    ISender sender,
    IMarkupContainer markupContainer,
    IPriceApplierService priceApplierService,
    ISettingsService settingsService
) : LrtNamedObjectBase(
    jobRepository,
    unitOfWork,
    publisher,
    logger)
{
    public const string LrtName = nameof(InvalidateStalePriceOptionsLrt); 
    public override string SystemName => LrtName;
    public override Type InputType => typeof(NoneInputState);
    public override Type StateType => typeof(InvalidateStalePriceOptionsState);
    public override IServiceDefinition ServiceDefinition => ServicesDefinitions.Pricing;
    public override string NameLocalizationKey => "lrt.invalidate.stale.price.options.name";
    public override string DescriptionLocalizationKey => "lrt.invalidate.stale.price.options.description";

    protected override async Task DoWork()
    {
        const int batchSize = 1000;

        while (true)
        {
            var processedCount = await UnitOfWork.ExecuteWithTransaction(
                TransactionalAttribute.ReadCommited(30, 3),
                async () =>
                {
                    var currentState = await GetStateAsync<InvalidateStalePriceOptionsState>()
                                       ?? new InvalidateStalePriceOptionsState();

                    var currentVersion = markupContainer.CurrentVersion;
                    var currentAppliersVersion = await priceApplierService
                        .GetCurrentConfigurationVersionAsync(CancellationToken);
                    var currentPricingSettingsVersion = (await settingsService
                        .GetOrDefault<PricingSetting>(CancellationToken)).Data.Version;

                    var items = await readRepository.Query
                        .Where(x => x.MarkupVersion != currentVersion
                                    || x.AppliersVersion != currentAppliersVersion
                                    || x.PricingSettingsVersion != currentPricingSettingsVersion)
                        .Take(batchSize)
                        .Select(x => new
                        {
                            x.PriceOfferId,
                            x.PriceOffer.ProductId,
                            x.PriceOffer.OfferForStorage
                        })
                        .ToListAsync(CancellationToken);

                    if (items.Count == 0) return 0;

                    var jobItems = items
                        .Select(x => PriceCandidateCalculationJob.Create(
                            x.ProductId,
                            x.OfferForStorage))
                        .DistinctBy(x => x.NaturalKey)
                        .ToList();

                    await sender.Send(
                        new TryEnqueueUniqJobCommand(jobItems),
                        CancellationToken);

                    await productPriceOptionRepository.DeleteManyAsync(
                        items.Select(x => x.PriceOfferId),
                        CancellationToken);

                    await UpdateState(new InvalidateStalePriceOptionsState
                    {
                        ProcessedRows = currentState.ProcessedRows + items.Count
                    });

                    return items.Count;
                },
                CancellationToken);

            if (processedCount < batchSize) break;
        }
    }
}
