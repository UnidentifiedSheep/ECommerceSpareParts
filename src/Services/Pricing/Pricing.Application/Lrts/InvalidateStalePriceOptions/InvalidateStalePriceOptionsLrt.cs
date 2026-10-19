using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Settings;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Lrts.PriceCandidateCalculation;
using Pricing.Entities;
using Pricing.Entities.Offers;
using Pricing.Entities.Settings;

namespace Pricing.Application.Lrts.InvalidateStalePriceOptions;

public class InvalidateStalePriceOptionsLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<InvalidateStalePriceOptionsLrt> logger) : LrtBase<NoneInputState, InvalidateStalePriceOptionsState>(
	transactionService,
	logger)
{
	public const string LrtName = nameof(InvalidateStalePriceOptionsLrt);

	public override string SystemName => LrtName;

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtInvalidateStalePriceOptionsNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtInvalidateStalePriceOptionsDescriptionMessage.Instance;

	protected override async Task DoWork(NoneInputState inputState)
	{
		const int batchSize = 1000;

		while (true)
		{
			var processedCount = await ExecuteInTransactionAsync(
				TransactionalAttribute.ReadCommitted(30, 3),
				async (context, cancellationToken) =>
				{
					var currentVersion = context.GetRequiredService<IMarkupContainer>().CurrentVersion;
					var currentAppliersVersion =
						await context.GetRequiredService<IPriceApplierService>().GetCurrentConfigurationVersionAsync(cancellationToken);
					var currentPricingSettingsVersion =
						(await context.GetRequiredService<ISettingsService>().GetOrDefault<PricingSetting>(cancellationToken)).Data.Version;

					var items = await context.GetRequiredService<IReadRepository<ProductPriceOption, Guid>>()
						.Query
						.Where(x =>
							x.MarkupVersion != currentVersion ||
							x.AppliersVersion != currentAppliersVersion ||
							x.PricingSettingsVersion != currentPricingSettingsVersion)
						.Take(batchSize)
						.Select(x => new
						{
							x.PriceOfferId,
							x.PriceOffer.ProductId,
							x.PriceOffer.OfferForStorage
						})
						.ToListAsync(cancellationToken);

					if (items.Count == 0)
						return 0;

					var jobItems = items
						.Select(x => context.GetRequiredService<IJobProvider<PriceCandidateCalculationLrt, PriceCandidateCalculationState>>().Create(
							new PriceCandidateCalculationState
							{
								ProductId = x.ProductId, StorageCode = x.OfferForStorage
							}))
						.DistinctBy(x => x.NaturalKey)
						.ToList();

					await context.GetRequiredService<IJobService>().TryEnqueueJobsAsync(jobItems, cancellationToken);

					await context.GetRequiredService<IProductPriceOptionRepository>().DeleteManyAsync(
						items.Select(x => x.PriceOfferId),
						cancellationToken);

					await SaveStateAsync(
						new InvalidateStalePriceOptionsState
						{
							ProcessedRows = State.ProcessedRows + items.Count
						});

					return items.Count;
				},
				CancellationToken);

			if (processedCount < batchSize)
				break;
		}
	}
}
