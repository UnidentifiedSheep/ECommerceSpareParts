using Abstractions.Interfaces;
using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities.Job;
using Localization.Abstractions.Interfaces;
using Main.Application.Interfaces.Persistence;
using Main.Application.Lrts.Base;
using Main.Entities.Producer;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProducerAliasesImport;

public class ProducerAliasImportLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IS3StorageService s3Service,
	IRepository<ProducerAlias, string> aliasRepository,
	IProducerRepository producerRepository,
	ILogger<ProducerAliasImportLrt> logger,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IOptions<S3BucketsOptions> bucketsOptions,
	IContextualStringLocalizer stringLocalizer)
	: CsvImportLrtBase<ProducerAliasesImportInputState, ProducerAliasesImportState,
		ProducerAliasImportLrt.ProducerAliasCsvDto, ProducerAliasImportLrt.ProducerAliasBatchItem>(
		jobRepository,
		bucketsOptions,
		unitOfWork,
		publisher,
		transactionService,
		logger,
		s3Service,
		stringLocalizer)
{
	public override string SystemName => nameof(ProducerAliasImportLrt);

	public override string NameLocalizationKey => "lrt.producer.other.names.import.name";

	public override string DescriptionLocalizationKey => "lrt.producer.other.names.import.description";

	protected override string GetTooManyErrorsLocalizationKey() =>
		"producer.too.many.errors.while.processing.batch";

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
			var message = ex is ILocalizableException localizableException
				? StringLocalizer.GetOrDefault(
					localizableException.MessageKey,
					localizableException.Arguments ?? []) ?? ex.Message
				: ex.Message;

			errors.Add(CreateError(rowIdx, message));
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

			errors.Add(CreateError(item.idx, StringLocalizer.Get("producer.other.name.duplicate.in.batch")));
		}

		var result = await TransactionService.ExecuteAsync(
			TransactionalAttribute.RetryOnConflict(20, 2),
			async (context, cancellationToken) =>
			{
				var existingAliases = (await aliasRepository.ListAsync(
						Criteria<ProducerAlias>
							.New()
							.Where(x => uniqueAliases.Contains(x.Alias))
							.Track(false)
							.Build(),
						cancellationToken))
					.Select(x => x.Alias)
					.ToHashSet();
				var producerNames = uniqueItems.Select(x => x.item.OriginalName).Distinct().ToList();
				var producers = (await producerRepository.ListAsync(
					Criteria<Producer>.New().Where(x => producerNames.Contains(x.Name)).Track(false).Build(),
					cancellationToken)).ToDictionary(x => x.Name);

				var transactionErrors = new List<CsvImportError>();
				var toAdd = new List<ProducerAlias>();
				foreach (var (idx, item) in uniqueItems)
				{
					if (existingAliases.Contains(item.Alias))
					{
						transactionErrors.Add(
							CreateError(idx, StringLocalizer.Get("producer.other.name.already.taken")));
						continue;
					}

					if (!producers.TryGetValue(item.OriginalName, out var producer))
					{
						transactionErrors.Add(
							CreateError(
								idx,
								StringLocalizer.Get("producer.other.name.producer.not.found.in.batch")));
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
