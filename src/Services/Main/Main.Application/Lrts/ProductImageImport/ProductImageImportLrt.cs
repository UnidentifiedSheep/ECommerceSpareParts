using Application.Common.Extensions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Enums;
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

namespace Main.Application.Lrts.ProductImageImport;

public class ProductImageImportLrt(
	IS3Service s3Service,
	IApplicationTransactionFactory transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductImageImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductImageImportInputState, ProductImageImportState,
		ProductImageImportLrt.ProductImageCsvDto, ProductImage>(
		bucketsOptions, transactionService,
		logger, s3Service, stringLocalizer)
{
	public const string Name = nameof(ProductImageImportLrt);

	public override string SystemName => Name;
	public override ILocalizableMessage NameLocalizationMessage => LrtProductImageImportNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage => LrtProductImageImportDescriptionMessage.Instance;
	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		ProductImageCsvDto row,
		ProductImageImportState state,
		List<CsvImportError> errors,
		out ProductImage item)
	{
		item = null!;
		if (row.ProductId <= 0)
		{
			errors.Add(CreateError(rowIdx, StringLocalizer.Get(LrtProductImageImportInvalidProductIdMessage.Instance)));
			return false;
		}

		try
		{
			item = ProductImage.CreateFromStorageKey(row.ProductId, row.FileName);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductImage item)> items,
		ProductImageImportState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0) return;

		var images = items.Select(x => x.item).DistinctBy(image => image.GetId()).ToArray();
		await using var transaction = TransactionFactory.Create();
		await transaction.WithSettings(TransactionalAttribute.RetryOnConflict(20, 2))
			.OnException(
				TransactionErrorKind.ForeignKeyViolation,
				error =>
				{
					Logger.LogWarning(
						error.Exception,
						"Missing product in image import batch. StartRow: {StartRow}, EndRow: {EndRow}",
						items[0].idx,
						items[^1].idx);
					Interrupt(
						StringLocalizer.Get(
							new LrtProductImageImportMissingReferenceMessage()
								.WithStartRow(items[0].idx)
								.WithEndRow(items[^1].idx)
								.WithConstraintName(error.ConstraintName ?? string.Empty)));
				})
			.OnException(
				TransactionErrorKind.UniqueConstraintViolation,
				error =>
				{
					Logger.LogWarning(
						error.Exception,
						"Image is already assigned to another product. StartRow: {StartRow}, EndRow: {EndRow}",
						items[0].idx,
						items[^1].idx);
					Interrupt(
						StringLocalizer.Get(
							new LrtProductImageImportStorageKeyAlreadyAssignedMessage()
								.WithStartRow(items[0].idx)
								.WithEndRow(items[^1].idx)
								.WithConstraintName(error.ConstraintName ?? string.Empty)));
				})
			.ExecuteAsync(
				async (context, cancellationToken) =>
				{
					await context.GetRequiredService<IProductRepository>().InsertProductImagesOnConflictDoNothingAsync(images, cancellationToken);
					context.GetRequiredService<IDomainEventScope>().AddRange(images.Select(image => image.ProductId)
						.Distinct()
						.Select(id => new ProductImageUpdatedDomainEvent(id)));
				},
				CancellationToken);

		Logger.LogInformation(
			"Product image import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, Errors: {Errors}",
			JobId, items[0].idx, items.Count, errors.Count);
	}

	public sealed record ProductImageCsvDto
	{
		[Name("ProductId")]
		public int ProductId { get; init; }

		[Name("FileName")]
		public required string FileName { get; init; }
	}
}
