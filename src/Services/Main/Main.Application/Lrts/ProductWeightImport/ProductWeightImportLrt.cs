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

namespace Main.Application.Lrts.ProductWeightImport;

public class ProductWeightImportLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductWeightImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductWeightImportInputState, ProductWeightImportState,
		ProductWeightImportLrt.ProductWeightCsvDto, ProductWeight>(
		jobRepository,
		bucketsOptions,
		unitOfWork,
		publisher,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	public const string Name = nameof(ProductWeightImportLrt);

	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage => LrtProductWeightImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage => LrtProductWeightImportDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		ProductWeightCsvDto row,
		ProductWeightImportState state,
		List<CsvImportError> errors,
		out ProductWeight item)
	{
		item = null!;
		if (!Enum.IsDefined(row.Unit))
		{
			errors.Add(CreateError(rowIdx, StringLocalizer.Get(LrtProductWeightImportInvalidUnitMessage.Instance)));
			return false;
		}

		try
		{
			item = ProductWeight.Create(row.ProductId, row.Weight, row.Unit);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductWeight item)> items,
		ProductWeightImportState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0) return;

		var productIds = items.Select(x => x.item.ProductId).Distinct().ToList();
		var result = await TransactionService.ExecuteAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			async (context, cancellationToken) =>
			{
				var products = await context.Repositories.Get<Product, int>().FindByIdsAsync(
					ids: productIds,
					criteria: Criteria<Product>.New()
						.Include(x => x.ProductWeight)
						.Track()
						.Build(),
					ct: cancellationToken);
				var newWeights = new Dictionary<int, ProductWeight>();
				var batchErrors = new List<CsvImportError>();

				foreach (var (rowIdx, weight) in items)
				{
					if (!products.TryGetValue(weight.ProductId, out var product))
					{
						batchErrors.Add(CreateError(rowIdx, StringLocalizer.Get(LrtProductWeightImportProductNotFoundMessage.Instance)));
						continue;
					}

					var targetWeight = product.ProductWeight;
					if (targetWeight is null && !newWeights.TryGetValue(weight.ProductId, out targetWeight))
					{
						newWeights.Add(weight.ProductId, weight);
						continue;
					}

					targetWeight.Update(weight.Weight, weight.Unit);
				}

				await context.UnitOfWork.AddRangeAsync(newWeights.Values, cancellationToken);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);
				return new ProductWeightImportBatchResult(newWeights.Count, batchErrors);
			},
			CancellationToken);

		errors.AddRange(result.Errors);
		Logger.LogInformation(
			"Product weight import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, Created: {Created}, Errors: {Errors}",
			JobId, items[0].idx, items.Count, result.Created, errors.Count);
	}

	private sealed record ProductWeightImportBatchResult(int Created, IReadOnlyList<CsvImportError> Errors);

	public sealed record ProductWeightCsvDto
	{
		[Name("ProductId")]
		public int ProductId { get; init; }

		[Name("Unit")]
		public WeightUnit Unit { get; init; }

		[Name("Weight")]
		public decimal Weight { get; init; }
	}
}
