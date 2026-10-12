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
using Main.Entities.Product;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductEanImport;

public class ProductEanImportLrt(
	IRepository<Job, Guid> jobRepository,
	IProductRepository productRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductEanImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductEanImportInputState, ProductEanImportState,
		ProductEanImportLrt.CsvRow, ProductEan>(
		jobRepository, bucketsOptions, unitOfWork, publisher, transactionService,
		logger, s3Service, stringLocalizer)
{
	protected override int BatchSize => 10_000;

	public override string SystemName => nameof(ProductEanImportLrt);
	public override ILocalizableMessage NameLocalizationMessage => LrtProductEanImportNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProductEanImportDescriptionMessage.Instance;
	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		CsvRow row,
		ProductEanImportState state,
		List<CsvImportError> errors,
		out ProductEan item)
	{
		item = null!;
		try
		{
			item = ProductEan.Create(row.ProductId, row.Ean);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductEan item)> items,
		ProductEanImportState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0) return;

		await TransactionService.ExecuteAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			(_, cancellationToken) => productRepository.InsertProductEansOnConflictDoNothingAsync(
				items.Select(x => x.item).ToArray(), cancellationToken),
			CancellationToken);

		Logger.LogInformation(
			"Product EAN import batch processed. JobId: {JobId}, BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}",
			JobId, items[0].idx, items.Count);
	}

	public sealed record CsvRow
	{
		[Name("ProductId")]
		public int ProductId { get; init; }

		[Name("Ean")]
		public required string Ean { get; init; }
	}
}
