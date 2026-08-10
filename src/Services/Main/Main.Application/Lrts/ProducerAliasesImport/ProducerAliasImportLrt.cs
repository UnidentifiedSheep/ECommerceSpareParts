using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Localization.Abstractions.Interfaces;
using Main.Application.Handlers.ProducerAliases;
using Main.Application.Handlers.Producers;
using Main.Application.Lrts.Base;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProducerAliasesImport;

public class ProducerAliasImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    ILogger<ProducerAliasImportLrt> logger,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    IOptions<S3BucketsOptions> bucketsOptions,
    IScopedStringLocalizer stringLocalizer
)
    : CsvImportLrtBase<
        ProducerAliasesImportInputState,
        ProducerAliasesImportState,
        ProducerAliasImportLrt.ProducerAliasCsvDto,
        CreateProducerAliasesBatchItem>(
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

    protected override string GetTooManyErrorsLocalizationKey()
    {
        return "producer.too.many.errors.while.processing.batch";
    }

    protected override bool TryProcessRow(
        int rowIdx,
        ProducerAliasCsvDto row,
        ProducerAliasesImportState state,
        List<CsvImportError> errors,
        out CreateProducerAliasesBatchItem item)
    {
        item = new CreateProducerAliasesBatchItem(
            row.Name,
            row.Alias);
        return true;
    }

    protected override async Task ProcessBatch(
        IReadOnlyList<(int idx, CreateProducerAliasesBatchItem item)> otherNames,
        ProducerAliasesImportState state,
        List<CsvImportError> errors)
    {
        if (otherNames.Count == 0) return;

        var firstIdx = otherNames[0].idx;
        var result = await sender.Send(
            new CreateProducerAliasesBatchCommand(otherNames.Select(x => x.item)),
            CancellationToken);

        foreach (var (idx, message) in result.Errors)
            errors.Add(
                new CsvImportError
                {
                    Message = message,
                    RowIdx = idx >= 0 && idx < otherNames.Count
                        ? otherNames[idx].idx
                        : firstIdx + idx
                });

        Logger.LogInformation(
            "Producer other names import batch processed. JobId: {JobId}, " +
            "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
            "Created: {Created}, Skipped: {Skipped}, Errors: {Errors}",
            JobId,
            firstIdx,
            otherNames.Count,
            result.Created,
            result.Skipped,
            result.Errors.Count);
    }

    public record ProducerAliasCsvDto
    {
        [Name("OriginalName", "Name")]
        public required string Name { get; init; }

        [Name("OtherName", "Alias")]
        public required string Alias { get; init; }
    }
}
