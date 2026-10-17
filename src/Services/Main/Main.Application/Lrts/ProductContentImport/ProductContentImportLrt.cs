using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Enums;
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

namespace Main.Application.Lrts.ProductContentImport;

public class ProductContentImportLrt(
	IRepository<Job, Guid> jobRepository,
	IProductRepository productRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductContentImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductContentImportInputState, ProductContentImportState,
		ProductContentImportLrt.ProductContentCsvDto, ProductContent>(
		jobRepository, bucketsOptions, unitOfWork, publisher, transactionService,
		logger, s3Service, stringLocalizer)
{
	public const string Name = nameof(ProductContentImportLrt);

	public override string SystemName => Name;
	public override ILocalizableMessage NameLocalizationMessage => LrtProductContentImportNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage => LrtProductContentImportDescriptionMessage.Instance;
	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		ProductContentCsvDto row,
		ProductContentImportState state,
		List<CsvImportError> errors,
		out ProductContent item)
	{
		item = null!;
		if (row.ParentId <= 0 || row.ChildId <= 0)
		{
			errors.Add(CreateError(rowIdx, StringLocalizer.Get(LrtProductContentImportInvalidIdMessage.Instance)));
			return false;
		}

		try
		{
			item = ProductContent.Create(row.ParentId, row.ChildId, row.Quantity);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductContent item)> items,
		ProductContentImportState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0) return;

		await TransactionService
			.Create(TransactionalAttribute.RetryOnConflict(20, 2))
			.OnException(
				TransactionErrorKind.ForeignKeyViolation,
				error =>
				{
					Logger.LogWarning(
						error.Exception,
						"Missing product in content import batch. StartRow: {StartRow}, EndRow: {EndRow}",
						items[0].idx,
						items[^1].idx);
					Interrupt(
						StringLocalizer.Get(
							new LrtProductContentImportMissingReferenceMessage()
								.WithStartRow(items[0].idx)
								.WithEndRow(items[^1].idx)
								.WithConstraintName(error.ConstraintName ?? string.Empty)));
				})
			.ExecuteAsync(
				(_, cancellationToken) =>
					productRepository.UpsertProductContentsAsync(
						items.Select(x => x.item).ToArray(),
						cancellationToken),
				CancellationToken);

		Logger.LogInformation(
			"Product content import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, Errors: {Errors}",
			JobId, items[0].idx, items.Count, errors.Count);
	}

	public sealed record ProductContentCsvDto
	{
		[Name("ParentId")]
		public int ParentId { get; init; }

		[Name("ChildId")]
		public int ChildId { get; init; }

		[Name("Quantity")]
		public int Quantity { get; init; }
	}
}
