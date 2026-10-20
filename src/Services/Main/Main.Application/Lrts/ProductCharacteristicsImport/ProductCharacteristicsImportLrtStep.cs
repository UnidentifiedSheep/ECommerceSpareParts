using Application.Common.Extensions;
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
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProductCharacteristicsImport;

public class ProductCharacteristicsImportLrtStep(
	IS3Service s3Service,
	IApplicationTransactionFactory transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<ProductCharacteristicsImportLrtStep> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProductCharacteristicsImportInputState, ProductCharacteristicsImportState,
		ProductCharacteristicsImportLrtStep.ProductCharacteristicCsvDto, ProductCharacteristic>(
		bucketsOptions, transactionService,
		logger, s3Service, stringLocalizer)
{
	public const string Name = nameof(ProductCharacteristicsImportLrtStep);

	private IReadOnlyDictionary<string, int> _characteristicIds = new Dictionary<string, int>();

	public override string SystemName => Name;
	public override ILocalizableMessage NameLocalizationMessage => LrtProductCharacteristicsImportStepNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage => LrtProductCharacteristicsImportStepDescriptionMessage.Instance;
	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ArticleImportTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override async Task BeforeRead(ProductCharacteristicsImportState state)
	{
		_characteristicIds = await ExecuteInScopeAsync((context, ct) => context.Repositories.GetForRead<Characteristic, int>().Query
			.Select(x => new { x.NormalizedName, x.Id })
			.ToDictionaryAsync(x => x.NormalizedName, x => x.Id, ct), CancellationToken);
	}

	protected override bool TryProcessRow(
		int rowIdx,
		ProductCharacteristicCsvDto row,
		ProductCharacteristicsImportState state,
		List<CsvImportError> errors,
		out ProductCharacteristic item)
	{
		item = null!;
		try
		{
			var normalizedName = Characteristic.NormalizeName(row.Characteristic);
			if (!_characteristicIds.TryGetValue(normalizedName, out var characteristicId))
			{
				errors.Add(CreateError(rowIdx, StringLocalizer.Get(CharacteristicNotFoundMessage.Instance)));
				return false;
			}

			item = ProductCharacteristic.Create(row.ProductId, characteristicId, row.Value);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProductCharacteristic item)> items,
		ProductCharacteristicsImportState state,
		List<CsvImportError> errors)
	{
		if (items.Count == 0) return;

		await using var transaction = TransactionFactory.Create();
		await transaction.WithSettings(TransactionalAttribute.RetryOnConflict(20, 2))
			.OnException(
				TransactionErrorKind.ForeignKeyViolation,
				error =>
				{
					Logger.LogWarning(
						error.Exception,
						"Missing reference in characteristics import batch. StartRow: {StartRow}, EndRow: {EndRow}",
						items[0].idx,
						items[^1].idx);
					Interrupt(
						StringLocalizer.Get(
							new LrtProductCharacteristicsImportMissingReferenceMessage()
								.WithStartRow(items[0].idx)
								.WithEndRow(items[^1].idx)
								.WithConstraintName(error.ConstraintName ?? string.Empty)));
				})
			.ExecuteAsync(
				(context, cancellationToken) =>
					context.GetRequiredService<IProductRepository>().UpsertProductCharacteristicsAsync(
						items.Select(x => x.item).ToArray(),
						cancellationToken),
				CancellationToken);

		Logger.LogInformation(
			"Product characteristics import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, Errors: {Errors}",
			JobId, items[0].idx, items.Count, errors.Count);
	}

	public sealed record ProductCharacteristicCsvDto
	{
		[Name("ProductId")]
		public int ProductId { get; init; }

		[Name("Characteristic")]
		public required string Characteristic { get; init; }

		[Name("Value")]
		public required string Value { get; init; }
	}
}
