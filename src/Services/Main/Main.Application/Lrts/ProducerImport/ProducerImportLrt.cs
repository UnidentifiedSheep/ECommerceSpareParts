using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Localization.Abstractions.Interfaces;
using Main.Application.Interfaces.Persistence;
using Main.Application.Lrts.Base;
using Main.Entities.Producer;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProducerImport;

public class ProducerImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    IProducerRepository producerRepository,
    ILogger<ProducerImportLrt> logger,
    IOptions<S3BucketsOptions> bucketsOptions,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    IScopedStringLocalizer stringLocalizer
) : CsvImportLrtBase<ProducerImportInputState, ProducerImportState, ProducerImportLrt.NewProducerCsvDto,
    Producer>(
    jobRepository,
    bucketsOptions,
    unitOfWork,
    publisher,
    transactionService,
    logger,
    s3Service,
    stringLocalizer)
{
    public override string SystemName => nameof(ProducerImportLrt);
    public override string NameLocalizationKey => "lrt.producer.import.name";
    public override string DescriptionLocalizationKey => "lrt.producer.import.description";

    protected override string GetTooManyErrorsLocalizationKey()
    {
        return "producer.too.many.errors.while.processing.batch";
    }

    protected override bool TryProcessRow(
        int rowIdx,
        NewProducerCsvDto row,
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
        IReadOnlyList<(int idx, Producer item)> producers,
        ProducerImportState state,
        List<CsvImportError> errors)
    {
        if (producers.Count == 0) return;

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

            errors.Add(CreateError(
                idx,
                StringLocalizer.Get("producer.duplicate.name.in.batch")));
        }

        var created = await TransactionService.ExecuteAsync(
            TransactionalAttribute.RetryOnConflict(20, 2),
            async (context, cancellationToken) =>
            {
                var existingNames = (await producerRepository.ListAsync(
                        Criteria<Producer>.New()
                            .Where(x => uniqueNames.Contains(x.Name))
                            .Track(false)
                            .Build(),
                        cancellationToken))
                    .Select(x => x.Name)
                    .ToHashSet();
                var toAdd = uniqueProducers
                    .Where(x => !existingNames.Contains(x.Name))
                    .ToList();

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

    public record NewProducerCsvDto
    {
        [Name("Name")]
        public required string Name { get; init; }

        [Name("Description")]
        [Optional]
        public string? Description { get; init; }
    }
}
