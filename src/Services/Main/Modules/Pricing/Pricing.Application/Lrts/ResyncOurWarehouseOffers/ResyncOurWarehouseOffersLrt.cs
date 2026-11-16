using Pricing.Application.Events;
using System.Globalization;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities;
using Pricing.Entities.Offers;

namespace Pricing.Application.Lrts.ResyncOurWarehouseOffers;

public class ResyncOurWarehouseOffersLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<ResyncOurWarehouseOffersLrt> logger)
	: LrtBase<NoneInputState, ResyncOurWarehouseOffersState>(
		transactionService,
		logger)
{
	public const string LrtName = nameof(ResyncOurWarehouseOffersLrt);

	private const int BatchSize = 1000;

	public override string SystemName => LrtName;

	public override ILocalizableMessage NameLocalizationMessage
		=> LrtResyncOurWarehouseOffersNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LrtResyncOurWarehouseOffersDescriptionMessage.Instance;

	protected override async Task DoWork(NoneInputState inputState)
	{
		while (true)
		{
			var processed = await ExecuteInTransactionAsync(
				TransactionalAttribute.ReadCommitted(60, 3),
				async (context, cancellationToken) =>
				{
					var snapshots = await context
						.GetRequiredService<IStorageContentSnapshotReader>()
						.GetAfterAsync(State.LastStorageContentId, BatchSize, cancellationToken);

					if (snapshots.Count == 0)
						return 0;

					var now = DateTime.UtcNow;
					var offers = snapshots
						.Select(x => PriceOffer.CreateForOurWarehouse(
							x.ProductId,
							x.CurrencyId,
							x.StorageCode,
							x.BuyPrice,
							x.StorageContentId.ToString(CultureInfo.InvariantCulture),
							x.AvailableCount,
							1,
							1,
							0,
							now))
						.ToList();

					context.IntegrationEventScope.AddRange(
						snapshots
							.Select(x => new { x.StorageCode, x.ProductId })
							.Distinct()
							.Select(x => new ProductPriceOffersUpdatedEvent
							{
								StorageCode = x.StorageCode, ProductId = x.ProductId
							}));

					await context
						.GetRequiredService<IPriceOfferRepository>()
						.UpsertOffersAsync(offers, cancellationToken);

					await SaveStateAsync(
						new ResyncOurWarehouseOffersState
						{
							LastStorageContentId = snapshots[^1].StorageContentId,
							ProcessedRows = State.ProcessedRows + snapshots.Count
						});

					return snapshots.Count;
				},
				CancellationToken);

			if (processed < BatchSize)
				break;
		}
	}
}
