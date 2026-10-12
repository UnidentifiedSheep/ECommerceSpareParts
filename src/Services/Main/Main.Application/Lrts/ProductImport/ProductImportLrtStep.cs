using Abstractions.Interfaces;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Lrts.Base;
using Main.Application.Models.Producer;
using Main.Entities;
using Main.Entities.Product;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductImport;

public class ProductImportLrtStep(
	IRepository<Job, Guid> jobRepository,
	IProducerLookupService producerLookupService,
	IProductRepository productRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductImportLrtStep> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductImportInputState, ProductImportState, ProductImportLrtStep.NewProductCsvDto,
		Product>(
		jobRepository,
		bucketsOptions,
		unitOfWork,
		publisher,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	private IProducerLookup _producerLookup = ProducerLookup.Empty;

	protected override int BatchSize => 10_000;

	public override string SystemName => nameof(ProductImportLrtStep);

	public override ILocalizableMessage NameLocalizationMessage => LrtProductImportStepNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductImportStepDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override async Task BeforeRead(ProductImportState state) =>
		_producerLookup = await producerLookupService.Load(CancellationToken);

	protected override bool TryProcessRow(
		int rowIdx,
		NewProductCsvDto row,
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

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, Product item)> products,
		ProductImportState state,
		List<CsvImportError> errors)
	{
		if (products.Count == 0)
			return;

		var firstIdx = products[0].idx;
		await TransactionService.ExecuteAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			(_, cancellationToken) => productRepository.InsertProductsOnConflictDoNothingAsync(
				products.Select(product => product.item).ToArray(), cancellationToken),
			CancellationToken);

		Logger.LogInformation(
			"Product import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}",
			JobId,
			firstIdx,
			products.Count);
	}

	public record NewProductCsvDto
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
