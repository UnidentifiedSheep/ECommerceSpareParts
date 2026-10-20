using Abstractions.Interfaces;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Lrts.Base;
using Main.Application.Models.Producer;
using Main.Entities;
using Main.Entities.DomainEvents.Product;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductCrossesImport;

public class ProductCrossesImportLrt(
	IS3Service s3Service,
	IApplicationTransactionFactory transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductCrossesImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductCrossesImportInputState, ProductCrossesImportState,
		ProductCrossesImportLrt.ProductCrossCsvDto, ProductCrossesImportLrt.ProductCrossBatchItem>(
		bucketsOptions,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	private IProducerLookup _producerLookup = ProducerLookup.Empty;

	public override string SystemName => nameof(ProductCrossesImportLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProductCrossesImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductCrossesImportDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override async Task BeforeRead(ProductCrossesImportState state) =>
		_producerLookup = await ExecuteInScopeAsync((context, ct) =>
			context.GetRequiredService<IProducerLookupService>().Load(ct), CancellationToken);

	protected override bool TryProcessRow(
		int rowIdx,
		ProductCrossCsvDto row,
		ProductCrossesImportState state,
		List<CsvImportError> errors,
		out ProductCrossBatchItem item)
	{
		item = null!;

		try
		{
			var producerId = _producerLookup.ResolveId(row.Producer);
			if (producerId is null)
			{
				errors.Add(
					CreateError(
						rowIdx,
						StringLocalizer.Get(
							new ArticleImportProducerNotFoundMessage().WithProducer(row.Producer))));
				return false;
			}

			var crossProducerId = _producerLookup.ResolveId(row.CrossProducer);
			if (crossProducerId is null)
			{
				errors.Add(
					CreateError(
						rowIdx,
						StringLocalizer.Get(
							new ArticleImportProducerNotFoundMessage().WithProducer(row.CrossProducer))));
				return false;
			}

			item = new ProductCrossBatchItem(
				new ProductKey(row.Sku, producerId.Value, row.Producer),
				new ProductKey(row.CrossSku, crossProducerId.Value, row.CrossProducer));

			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductCrossBatchItem item)> items,
		ProductCrossesImportState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0)
			return;

		var firstIdx = items[0].idx;
		var errorsBeforeBatch = errors.Count;
		var productIds = await ExecuteInScopeAsync((context, ct) =>
			context.GetRequiredService<IProductRepository>().GetProductIdsByKeys(
			items.SelectMany(x => new[]
			{
				x.item.Product.ToTuple(),
				x.item.CrossProduct.ToTuple()
			}), ct), CancellationToken);

		var crosses = new HashSet<(int ProductId, int CrossProductId)>();

		foreach (var (idx, item) in items)
		{
			if (!productIds.TryGetValue(item.Product.ToTuple(), out var productId))
			{
				errors.Add(
					CreateError(
						idx,
						StringLocalizer.Get(
							new ArticleCrossImportProductNotFoundMessage()
								.WithArticleNumber(item.Product.Sku)
								.WithProducer(item.Product.Producer))));
				continue;
			}

			if (!productIds.TryGetValue(item.CrossProduct.ToTuple(), out var crossProductId))
			{
				errors.Add(
					CreateError(
						idx,
						StringLocalizer.Get(
							new ArticleCrossImportProductNotFoundMessage()
								.WithArticleNumber(item.CrossProduct.Sku)
								.WithProducer(item.CrossProduct.Producer))));
				continue;
			}

			if (productId == crossProductId)
			{
				errors.Add(
					CreateError(
						idx,
						StringLocalizer.Get(ArticleLinkageArticleCannotEqualCrossArticleMessage.Instance)));
				continue;
			}

			var cross = productId < crossProductId
				? (productId, crossProductId)
				: (crossProductId, productId);

			crosses.Add(cross);
		}

		if (crosses.Count > 0)
			await ExecuteInTransactionAsync(
				TransactionalAttribute.ReadCommitted(20, 2),
				async (context, cancellationToken) =>
				{
					var entities = crosses
						.Select(x => ProductCross.Create(x.ProductId, x.CrossProductId))
						.ToList();
					await context.GetRequiredService<IProductRepository>().InsertProductCrossesOnConflictDoNothingAsync(entities, cancellationToken);
					context.GetRequiredService<IDomainEventScope>().AddRange(
						entities
							.SelectMany(x => new[]
							{
								x.LeftProductId, x.RightProductId
							})
							.Distinct()
							.Select(x => new ProductLinkageUpdatedDomainEvent(x)));
				},
				CancellationToken);

		Logger.LogInformation(
			"Product crosses import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Upserted: {Upserted}, Errors: {Errors}",
			JobId,
			firstIdx,
			items.Count,
			crosses.Count,
			errors.Count - errorsBeforeBatch);
	}

	public sealed record ProductKey(string Sku, int ProducerId, string Producer)
	{
		public string NormalizedSku { get; } = new Sku(Sku).NormalizedValue;

		public (string NormalizedSku, int ProducerId) ToTuple() => (NormalizedSku, ProducerId);
	}

	public sealed record ProductCrossBatchItem(ProductKey Product, ProductKey CrossProduct);

	public sealed record ProductCrossCsvDto
	{
		[Name("Sku")]
		public required string Sku { get; init; }

		[Name("Producer")]
		public required string Producer { get; init; }

		[Name("CrossSku")]
		public required string CrossSku { get; init; }

		[Name("CrossProducer")]
		public required string CrossProducer { get; init; }
	}
}
