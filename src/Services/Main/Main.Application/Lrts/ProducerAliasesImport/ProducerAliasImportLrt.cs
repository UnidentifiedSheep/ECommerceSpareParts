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

namespace Main.Application.Lrts.ProducerAliasesImport;

public class ProducerAliasImportLrt(
	IS3Service s3Service,
	ILogger<ProducerAliasImportLrt> logger,
	IApplicationTransactionFactory transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	IContextualLocalizer stringLocalizer)
	: CsvImportLrtBase<ProducerAliasesImportInputState, ProducerAliasesImportState,
		ProducerAliasImportLrt.ProducerAliasCsvDto, ProducerAliasImportLrt.ProducerAliasBatchItem>(
		bucketsOptions,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	public override string SystemName => nameof(ProducerAliasImportLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtProducerOtherNamesImportNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtProducerOtherNamesImportDescriptionMessage.Instance;

	protected override ILocalizableMessage GetTooManyErrorsLocalizationMessage =>
		ProducerTooManyErrorsWhileProcessingBatchMessage.Instance;

	protected override bool TryProcessRow(
		int rowIdx,
		ProducerAliasCsvDto row,
		ProducerAliasesImportState state,
		List<CsvImportError> errors,
		out ProducerAliasBatchItem item)
	{
		item = null!;
		try
		{
			var alias = ProducerAlias.Create(0, row.Alias);
			item = new ProducerAliasBatchItem(Producer.ToNormalizedName(row.Name), alias.Alias);
			return true;
		}
		catch (Exception ex)
		{
			errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
			return false;
		}
	}

	protected override async Task ProcessBatch(
		IReadOnlyList<(int idx, ProducerAliasBatchItem item)> aliases,
		ProducerAliasesImportState state,
		List<CsvImportError> errors)
	{
		if (aliases.Count == 0)
			return;

		var firstIdx = aliases[0].idx;
		var errorsBeforeBatch = errors.Count;
		var uniqueAliases = new HashSet<string>();
		var uniqueItems = new List<(int idx, ProducerAliasBatchItem item)>();
		foreach (var item in aliases)
		{
			if (uniqueAliases.Add(item.item.Alias))
			{
				uniqueItems.Add(item);
				continue;
			}

			errors.Add(
				CreateError(
					item.idx,
					StringLocalizer.Get(ProducerOtherNameDuplicateInBatchMessage.Instance)));
		}

		var result = await ExecuteInTransactionAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			async (context, cancellationToken) =>
			{
				var existingAliases = (await context.GetRequiredService<IRepository<ProducerAlias, string>>().ListAsync(
						Criteria<ProducerAlias>
							.New()
							.Where(x => uniqueAliases.Contains(x.Alias))
							.Track(false)
							.Build(),
						cancellationToken))
					.Select(x => x.Alias)
					.ToHashSet();
				var producerNames = uniqueItems.Select(x => x.item.OriginalName).Distinct().ToList();
				var producers = (await context.GetRequiredService<IProducerRepository>().ListAsync(
					Criteria<Producer>.New().Where(x => producerNames.Contains(x.Name)).Track(false).Build(),
					cancellationToken)).ToDictionary(x => x.Name);

				var transactionErrors = new List<CsvImportError>();
				var toAdd = new List<ProducerAlias>();
				foreach (var (idx, item) in uniqueItems)
				{
					if (existingAliases.Contains(item.Alias))
					{
						transactionErrors.Add(
							CreateError(
								idx,
								StringLocalizer.Get(ProducerOtherNameAlreadyTakenMessage.Instance)));
						continue;
					}

					if (!producers.TryGetValue(item.OriginalName, out var producer))
					{
						transactionErrors.Add(
							CreateError(
								idx,
								StringLocalizer.Get(
									ProducerOtherNameProducerNotFoundInBatchMessage.Instance)));
						continue;
					}

					toAdd.Add(ProducerAlias.Create(producer.Id, item.Alias));
				}

				await context.UnitOfWork.AddRangeAsync(toAdd, cancellationToken);
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);
				return new ProducerAliasBatchResult(toAdd.Count, transactionErrors);
			},
			CancellationToken);

		errors.AddRange(result.Errors);

		Logger.LogInformation(
			"Producer other names import batch processed. JobId: {JobId}, " +
			"BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
			"Created: {Created}, Skipped: {Skipped}, Errors: {Errors}",
			JobId,
			firstIdx,
			aliases.Count,
			result.Created,
			aliases.Count - result.Created,
			errors.Count - errorsBeforeBatch);
	}

	public sealed record ProducerAliasBatchItem(string OriginalName, string Alias);

	private sealed record ProducerAliasBatchResult(int Created, IReadOnlyList<CsvImportError> Errors);

	public record ProducerAliasCsvDto
	{
		[Name("OriginalName", "Name")]
		public required string Name { get; init; }

		[Name("OtherName", "Alias")]
		public required string Alias { get; init; }
	}
}
