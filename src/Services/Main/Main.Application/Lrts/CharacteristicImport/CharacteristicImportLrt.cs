using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Lrts.Base;
using Main.Entities;
using Main.Entities.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.CharacteristicImport;

public sealed class CharacteristicImportLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IS3Service s3Service,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<CharacteristicImportLrt> logger,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<CharacteristicImportInputState, CharacteristicImportState,
		CharacteristicImportLrt.CharacteristicCsvDto, Characteristic>(
		jobRepository,
		bucketsOptions,
		unitOfWork,
		publisher,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	public const string Name = nameof(CharacteristicImportLrt);

	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage => LrtCharacteristicImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage => LrtCharacteristicImportDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		CharacteristicImportTooManyErrorsMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		CharacteristicCsvDto row,
		CharacteristicImportState state,
		List<CsvImportError> errors,
		out Characteristic item)
	{
		item = null!;
		try
		{
			item = Characteristic.Create(row.Name);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, Characteristic item)> characteristics,
		CharacteristicImportState state,
		List<CsvImportError> errors)
	{
		if (characteristics.Count == 0) return;

		var uniqueCharacteristics = characteristics
			.Select(characteristic => characteristic.item)
			.DistinctBy(characteristic => characteristic.NormalizedName)
			.ToList();
		var uniqueNames = uniqueCharacteristics.Select(characteristic => characteristic.NormalizedName).ToArray();

		var created = await TransactionService.ExecuteAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			async (context, cancellationToken) =>
			{
				var existingNames = (await context.Repositories
					.GetForRead<Characteristic, int>()
					.Query
					.Where(characteristic => uniqueNames.Contains(characteristic.NormalizedName))
					.Select(characteristic => characteristic.NormalizedName)
					.ToListAsync(cancellationToken))
					.ToHashSet();

				var toCreate = uniqueCharacteristics
					.Where(characteristic => !existingNames.Contains(characteristic.NormalizedName))
					.ToList();

				await context.UnitOfWork.AddRangeAsync(toCreate, cancellationToken);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);

				return toCreate.Count;
			},
			CancellationToken);

		Logger.LogInformation(
			"Characteristic import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Created: {Created}, Skipped: {Skipped}",
			JobId,
			characteristics[0].idx,
			characteristics.Count,
			created,
			characteristics.Count - created);
	}

	public sealed record CharacteristicCsvDto
	{
		[Name("Name")]
		public required string Name { get; init; }
	}
}
