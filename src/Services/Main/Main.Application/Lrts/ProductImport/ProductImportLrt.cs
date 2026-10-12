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

public class ProductImportLrt(
	IRepository<Job, Guid> jobRepository,
	IProducerLookupService producerLookupService,
	IProductRepository productRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductImportInputState, ProductImportState, ProductImportLrt.NewProductCsvDto,
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

	protected override int BatchSize => 1000;

	public override string SystemName => nameof(ProductImportLrt);

	public override ILocalizableMessage NameLocalizationMessage => LrtProductImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductImportDescriptionMessage.Instance;

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
		var uniqueKeys = new HashSet<(string NormalizedSku, int ProducerId)>();
		var uniqueProducts = new List<Product>();
		foreach (var (_, product) in products)
		{
			if (uniqueKeys.Add(GetProductKey(product)))
				uniqueProducts.Add(product);
		}

		var result = await TransactionService.ExecuteAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			async (context, cancellationToken) =>
			{
				var existingKeys = await productRepository.GetExistingProductKeys(
					uniqueKeys,
					cancellationToken);
				var toCreate = uniqueProducts
					.Where(product => !existingKeys.Contains(GetProductKey(product)))
					.ToList();

				await context.UnitOfWork.AddRangeAsync(toCreate, cancellationToken);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);

				return toCreate.Count;
			},
			CancellationToken);

		Logger.LogInformation(
			"Product import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Created: {Created}, Skipped: {Skipped}",
			JobId,
			firstIdx,
			products.Count,
			result,
			products.Count - result);
	}

	private static (string NormalizedSku, int ProducerId) GetProductKey(Product product) =>
		(product.Sku.NormalizedValue, product.ProducerId);

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
