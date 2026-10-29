using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Products;
using Main.Application.Interfaces.Services;
using Main.Application.Lrts.Base;
using Main.Application.Models.Producer;
using Main.Entities;
using Main.Entities.Product;
using Main.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductImport;

public class ProductImportLrt(
	IS3Service s3Service,
	IApplicationTransactionFactory transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductImportInputState, ProductImportState, ProductImportLrt.ProductCsvDto,
		Product>(
		bucketsOptions,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	private IProducerLookup _producerLookup = ProducerLookup.Empty;

	protected override int BatchSize => 10_000;

	public override string SystemName => nameof(ProductImportLrt);

	public override ILocalizableMessage NameLocalizationMessage => LrtProductImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductImportDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override async Task BeforeRead(ProductImportState state) =>
		_producerLookup = await ExecuteInScopeAsync((context, ct) =>
			context.GetRequiredService<IProducerLookupService>().Load(ct), CancellationToken);

	protected override bool TryProcessRow(
		int rowIdx,
		ProductCsvDto row,
		ProductImportState state,
		List<CsvImportError> errors,
		out Product item)
	{
		item = null!;
		try
		{
			var producerId = _producerLookup.ResolveId(row.Producer);
			if (producerId is null)
			{
				errors.Add(CreateError(
					rowIdx,
					StringLocalizer.Get(
						new ArticleImportProducerNotFoundMessage().WithProducer(row.Producer))));
				return false;
			}

			item = Product.Create(
				row.Sku,
				row.Name,
				producerId.Value,
				row.Description);
			item.SetIndicator(row.Indicator);
			item.SetProductGroup(row.GroupId);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			item = null!;
			return false;
		}
	}

	protected override Task SaveBatchAsync(
		IReadOnlyList<(int idx, Product item)> products,
		ProductImportState state,
		CsvImportCheckpoint checkpoint,
		List<CsvImportError> errors) =>
		ExecuteInTransactionAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			(_, _) => base.SaveBatchAsync(products, state, checkpoint, errors),
			CancellationToken);

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, Product item)> products,
		ProductImportState state,
		List<CsvImportError> errors)
	{
		if (products.Count == 0)
			return;

		var firstIdx = products[0].idx;
		var ids = await TransactionContext.GetRequiredService<IProductRepository>()
			.InsertProductsOnConflictDoNothingAsync(products.Select(product => product.item).ToArray(), CancellationToken);
		await TransactionContext.GetRequiredService<IProductEventPublisher>()
			.PublishAsync(ids, ProductEventParts.All, CancellationToken);

		Logger.LogInformation(
			"Product import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, Created: {Created}",
			JobId,
			firstIdx,
			products.Count,
			ids.Count);
	}

	public record ProductCsvDto
	{
		[Name("Sku")]
		public required string Sku { get; init; }

		[Name("Name")]
		public required string Name { get; init; }

		[Name("Producer", "ProducerName")]
		public required string Producer { get; init; }

		[Optional]
		[Name("Description")]
		public string? Description { get; init; }

		[Optional]
		[Name("Indicator")]
		public string? Indicator { get; init; }

		[Optional]
		[Name("GroupId")]
		public int? GroupId { get; init; }
	}
}
