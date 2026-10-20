using Abstractions.Interfaces;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Lrts.Base;
using Main.Entities;
using Main.Entities.Producer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.ProducerImport;

public class ProducerImportLrt(
	IS3Service s3Service,
	ILogger<ProducerImportLrt> logger,
	IOptions<S3BucketsOptions> bucketsOptions,
	IApplicationTransactionFactory transactionService,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProducerImportInputState, ProducerImportState, ProducerImportLrt.ProducerCsvDto,
		Producer>(
		bucketsOptions,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	public override string SystemName => nameof(ProducerImportLrt);

	public override ILocalizableMessage NameLocalizationMessage => LrtProducerImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProducerImportDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ProducerTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		ProducerCsvDto row,
		ProducerImportState state,
		List<CsvImportError> errors,
		out Producer item)
	{
		item = null!;
		try
		{
			item = Producer.Create(row.Name, row.Description);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, Producer item)> producers,
		ProducerImportState state,
		List<CsvImportError> errors)
	{
		if (producers.Count == 0)
			return;

		var firstIdx = producers[0].idx;
		var errorsBeforeBatch = errors.Count;

		var uniqueNames = new HashSet<string>();
		var uniqueProducers = new List<Producer>();
		foreach (var (idx, producer) in producers)
		{
			if (uniqueNames.Add(producer.Name))
			{
				uniqueProducers.Add(producer);
				continue;
			}

			errors.Add(CreateError(idx, StringLocalizer.Get(ProducerDuplicateNameInBatchMessage.Instance)));
		}

		var created = await ExecuteInTransactionAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			async (context, cancellationToken) =>
			{
				var existingNames = (await context.GetRequiredService<IProducerRepository>().ListAsync(
						Criteria<Producer>
							.New()
							.Where(x => uniqueNames.Contains(x.Name))
							.Track(false)
							.Build(),
						cancellationToken))
					.Select(x => x.Name)
					.ToHashSet();
				var toAdd = uniqueProducers.Where(x => !existingNames.Contains(x.Name)).ToList();

				await context.UnitOfWork.AddRangeAsync(toAdd, cancellationToken);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);
				return toAdd.Count;
			},
			CancellationToken);

		Logger.LogInformation(
			"Producer import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Created: {Created}, Skipped: {Skipped}, Errors: {Errors}",
			JobId,
			firstIdx,
			producers.Count,
			created,
			uniqueProducers.Count - created,
			errors.Count - errorsBeforeBatch);
	}

	public record ProducerCsvDto
	{
		[Name("Name")]
		public required string Name { get; init; }

		[Name("Description")]
		[Optional]
		public string? Description { get; init; }
	}
}
