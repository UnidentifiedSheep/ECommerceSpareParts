using Abstractions.Interfaces;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Lrts.Base;
using Main.Entities;
using Main.Entities.DomainEvents.Product;
using Main.Entities.Product;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductCrossesByIdsDelete;

public class ProductCrossesByIdsDeleteLrt(
	IS3Service s3Service,
	IApplicationTransactionFactory transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductCrossesByIdsDeleteLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductCrossesByIdsDeleteInputState, ProductCrossesByIdsDeleteState,
		ProductCrossesByIdsDeleteLrt.ProductCrossByIdsCsvDto, ProductCross>(
		bucketsOptions,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	protected override int BatchSize => 10_000;

	public override string SystemName => nameof(ProductCrossesByIdsDeleteLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProductCrossesByIdsDeleteNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductCrossesByIdsDeleteDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		ProductCrossByIdsCsvDto row,
		ProductCrossesByIdsDeleteState state,
		List<CsvImportError> errors,
		out ProductCross item)
	{
		item = null!;
		if (row.Id <= 0 || row.CrossId <= 0)
		{
			errors.Add(CreateError(
				rowIdx,
				StringLocalizer.Get(LrtProductCrossesByIdsImportInvalidIdMessage.Instance)));
			return false;
		}

		try
		{
			item = ProductCross.Create(row.Id, row.CrossId);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductCross item)> items,
		ProductCrossesByIdsDeleteState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0)
			return;

		var crosses = items.Select(x => x.item)
			.DistinctBy(x => (x.LeftProductId, x.RightProductId))
			.ToArray();

		await ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(20, 2),
			async (context, cancellationToken) =>
			{
				await context.GetRequiredService<IProductRepository>().DeleteProductCrossesAsync(crosses, cancellationToken);
				context.GetRequiredService<IDomainEventScope>().AddRange(crosses
					.SelectMany(x => new[] { x.LeftProductId, x.RightProductId })
					.Distinct()
					.Select(id => new ProductLinkageUpdatedDomainEvent(id)));
			},
			CancellationToken);

		Logger.LogInformation(
			"Product crosses by IDs delete batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Candidates: {Candidates}, Errors: {Errors}",
			JobId,
			items[0].idx,
			items.Count,
			crosses.Length,
			errors.Count);
	}

	public sealed record ProductCrossByIdsCsvDto
	{
		[Name("Id")]
		public int Id { get; init; }

		[Name("CrossId")]
		public int CrossId { get; init; }
	}
}
