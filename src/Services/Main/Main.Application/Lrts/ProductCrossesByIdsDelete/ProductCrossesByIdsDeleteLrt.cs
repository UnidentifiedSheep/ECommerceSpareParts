using Abstractions.Interfaces;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Lrts.Base;
using Main.Entities;
using Main.Entities.DomainEvents.Product;
using Main.Entities.Product;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductCrossesByIdsDelete;

public class ProductCrossesByIdsDeleteLrt(
	IRepository<Job, Guid> jobRepository,
	IProductRepository productRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IDomainEventScope domainEventScope,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductCrossesByIdsDeleteLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductCrossesByIdsDeleteInputState, ProductCrossesByIdsDeleteState,
		ProductCrossesByIdsDeleteLrt.CsvRow, ProductCross>(
		jobRepository,
		bucketsOptions,
		unitOfWork,
		publisher,
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
		CsvRow row,
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

		await TransactionService.ExecuteAsync(
			TransactionalAttribute.ReadCommitted(20, 2),
			async (_, cancellationToken) =>
			{
				await productRepository.DeleteProductCrossesAsync(crosses, cancellationToken);
				domainEventScope.AddRange(crosses
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

	public sealed record CsvRow
	{
		[Name("Id")]
		public int Id { get; init; }

		[Name("CrossId")]
		public int CrossId { get; init; }
	}
}
