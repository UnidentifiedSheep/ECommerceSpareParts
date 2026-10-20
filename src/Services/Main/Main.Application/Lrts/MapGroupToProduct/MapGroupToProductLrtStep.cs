using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Lrts.Base;
using Main.Application.Models.Product;
using Main.Application.Models.Producer;
using Main.Entities;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.MapGroupToProduct;

public class MapGroupToProductLrtStep(
	IS3Service s3Service,
	IApplicationTransactionFactory transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<MapGroupToProductLrtStep> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<MapGroupToProductInputState, MapGroupToProductState,
		MapGroupToProductLrtStep.ProductGroupMappingCsvDto, ProductGroupAssignment>(
		bucketsOptions, transactionService,
		logger, s3Service, stringLocalizer)
{
	private IProducerLookup _producerLookup = ProducerLookup.Empty;
	private IReadOnlyDictionary<string, int> _groupIds = new Dictionary<string, int>();

	protected override int BatchSize => 10_000;

	public override string SystemName => nameof(MapGroupToProductLrtStep);
	public override ILocalizableMessage NameLocalizationMessage => LrtMapGroupToProductStepNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage => LrtMapGroupToProductStepDescriptionMessage.Instance;
	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override async Task BeforeRead(MapGroupToProductState state)
	{
		(_producerLookup, _groupIds) = await ExecuteInScopeAsync(async (context, ct) =>
		{
			var lookup = await context.GetRequiredService<IProducerLookupService>().Load(ct);
			var groups = await context.Repositories.GetForRead<ProductGroup, int>().Query
				.Select(x => new { x.NormalizedName, x.Id })
				.ToDictionaryAsync(x => x.NormalizedName, x => x.Id, ct);
			return (lookup, groups);
		}, CancellationToken);
	}

	protected override bool TryProcessRow(
		int rowIdx,
		ProductGroupMappingCsvDto row,
		MapGroupToProductState state,
		List<CsvImportError> errors,
		out ProductGroupAssignment item)
	{
		item = null!;
		try
		{
			var producerId = _producerLookup.ResolveId(row.Producer);
			if (producerId is null)
			{
				errors.Add(CreateError(rowIdx, StringLocalizer.Get(
					new ArticleImportProducerNotFoundMessage().WithProducer(row.Producer))));
				return false;
			}

			var normalizedSku = new Sku(row.Sku).NormalizedValue;
			var normalizedGroupName = ProductGroup.NormalizeName(row.ProductGroup);
			if (!_groupIds.TryGetValue(normalizedGroupName, out var groupId))
			{
				errors.Add(CreateError(rowIdx, StringLocalizer.Get(ProductGroupNotFoundMessage.Instance)));
				return false;
			}

			item = new ProductGroupAssignment(
				normalizedSku,
				producerId.Value,
				groupId);

			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductGroupAssignment item)> items,
		MapGroupToProductState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0) return;

		var assignments = new Dictionary<(string NormalizedSku, int ProducerId), ProductGroupAssignment>();
		foreach (var (_, item) in items)
			assignments[(item.NormalizedSku, item.ProducerId)] = item;

		await ExecuteInTransactionAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			(context, cancellationToken) => context.GetRequiredService<IProductRepository>()
				.UpdateProductGroupsAsync(
					assignments.Values,
					cancellationToken),
			CancellationToken);

		Logger.LogInformation(
			"Product group mapping batch processed. JobId: {JobId}, BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}",
			JobId, items[0].idx, assignments.Count);
	}

	public sealed record ProductGroupMappingCsvDto
	{
		[Name("Sku")]
		public required string Sku { get; init; }
		[Name("Producer")]
		public required string Producer { get; init; }
		[Name("ProductGroup")]
		public required string ProductGroup { get; init; }
	}
}
