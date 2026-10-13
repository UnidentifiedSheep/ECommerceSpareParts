using Abstractions.Interfaces;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Enums.Units;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Lrts.Base;
using Main.Entities;
using Main.Entities.Product;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductSizesImport;

public class ProductSizesImportLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductSizesImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductSizesImportInputState, ProductSizesImportState,
		ProductSizesImportLrt.CsvRow, ProductSize>(
		jobRepository,
		bucketsOptions,
		unitOfWork,
		publisher,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	public override string SystemName => nameof(ProductSizesImportLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProductSizesImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductSizesImportDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		CsvRow row,
		ProductSizesImportState state,
		List<CsvImportError> errors,
		out ProductSize item)
	{
		item = null!;
		if (row.ProductId <= 0)
		{
			errors.Add(CreateError(
				rowIdx,
				StringLocalizer.Get(LrtProductSizesImportInvalidProductIdMessage.Instance)));
			return false;
		}

		if (!Enum.IsDefined(row.Unit))
		{
			errors.Add(CreateError(
				rowIdx,
				StringLocalizer.Get(LrtProductSizesImportInvalidUnitMessage.Instance)));
			return false;
		}

		try
		{
			item = ProductSize.Create(
				row.ProductId,
				row.Length,
				row.Width,
				row.Height,
				row.Unit);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductSize item)> items,
		ProductSizesImportState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0)
			return;

		var productIds = items.Select(x => x.item.ProductId).Distinct().ToList();
		var result = await TransactionService.ExecuteAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			async (context, cancellationToken) =>
			{
				var products = await context
					.Repositories
					.Get<Product, int>()
					.FindByIdsAsync(
						ids: productIds,
						criteria: Criteria<Product>.New().Include(x => x.ProductSize).Track().Build(),
						ct: cancellationToken);

				var newSizes = new Dictionary<int, ProductSize>();
				var batchErrors = new List<CsvImportError>();

				foreach (var (rowIdx, size) in items)
				{
					if (!products.TryGetValue(size.ProductId, out var product))
					{
						batchErrors.Add(CreateError(
							rowIdx,
							StringLocalizer.Get(LrtProductSizesImportProductNotFoundMessage.Instance)));
						continue;
					}

					var targetSize = product.ProductSize;
					if (targetSize is null && !newSizes.TryGetValue(size.ProductId, out targetSize))
					{
						newSizes.Add(size.ProductId, size);
						continue;
					}

					targetSize.SetLength(size.Length);
					targetSize.SetWidth(size.Width);
					targetSize.SetHeight(size.Height);
					targetSize.SetUnit(size.Unit);
				}

				await context.UnitOfWork.AddRangeAsync(newSizes.Values, cancellationToken);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);
				return new ProductSizesImportBatchResult(newSizes.Count, batchErrors);
			},
			CancellationToken);

		errors.AddRange(result.Errors);
		Logger.LogInformation(
			"Product sizes import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Created: {Created}, Errors: {Errors}",
			JobId,
			items[0].idx,
			items.Count,
			result.Created,
			errors.Count);
	}

	private sealed record ProductSizesImportBatchResult(int Created, IReadOnlyList<CsvImportError> Errors);

	public sealed record CsvRow
	{
		[Name("ProductId")]
		public int ProductId { get; init; }

		[Name("Unit")]
		public DimensionUnit Unit { get; init; }

		[Name("Length")]
		public decimal Length { get; init; }

		[Name("Width")]
		public decimal Width { get; init; }

		[Name("Height")]
		public decimal Height { get; init; }
	}
}
