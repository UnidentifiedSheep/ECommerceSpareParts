using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Handlers.Jobs;
using Application.Common.Interfaces.Persistence;
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
    IApplicationTransactionService transactionService,
    ILogger<InvalidateStalePriceOptionsLrt> logger,
    IReadRepository<ProductPriceOption, Guid> readRepository,
    IProductPriceOptionRepository productPriceOptionRepository,
    ISender sender,
    IMarkupContainer markupContainer,
    IPriceApplierService priceApplierService,
    ISettingsService settingsService
) : LrtBase<NoneInputState, InvalidateStalePriceOptionsState>(
    jobRepository,
    unitOfWork,
    publisher,
    transactionService,
    logger)
{
    public const string LrtName = nameof(InvalidateStalePriceOptionsLrt); 
    public override string SystemName => LrtName;
    public override string NameLocalizationKey => "lrt.invalidate.stale.price.options.name";
    public override string DescriptionLocalizationKey => "lrt.invalidate.stale.price.options.description";

    protected override async Task DoWork()
    {
        const int batchSize = 1000;

        while (true)
        {
            var processedCount = await TransactionService.ExecuteAsync(
                TransactionalAttribute.ReadCommitted(30, 3),
                async (_, cancellationToken) =>
                {
                    var currentVersion = markupContainer.CurrentVersion;
                    var currentAppliersVersion = await priceApplierService
                        .GetCurrentConfigurationVersionAsync(cancellationToken);
                    var currentPricingSettingsVersion = (await settingsService
                        .GetOrDefault<PricingSetting>(cancellationToken)).Data.Version;

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
                        .ToListAsync(cancellationToken);

                    if (items.Count == 0) return 0;

                    var jobItems = items
                        .Select(x => PriceCandidateCalculationJob.Create(
                            x.ProductId,
                            x.OfferForStorage))
                        .DistinctBy(x => x.NaturalKey)
                        .ToList();

                    await sender.Send(
                        new TryEnqueueUniqJobCommand(jobItems),
                        cancellationToken);

                    await productPriceOptionRepository.DeleteManyAsync(
                        items.Select(x => x.PriceOfferId),
                        cancellationToken);

                    await SaveStateAsync(new InvalidateStalePriceOptionsState
                    {
                        ProcessedRows = State.ProcessedRows + items.Count
                    });

                    return items.Count;
                },
                CancellationToken);

            if (processedCount < batchSize) break;
        }
    }
}
