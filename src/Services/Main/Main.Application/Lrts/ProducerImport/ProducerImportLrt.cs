using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers;
using Main.Application.Lrts.Base;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProducerImport;

public class ProducerImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    ILogger<ProducerImportLrt> logger,
    IOptions<S3BucketsOptions> bucketsOptions,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    IScopedStringLocalizer stringLocalizer
) : CsvImportLrtBase<ProducerImportInputState, ProducerImportState, ProducerImportLrt.NewProducerCsvDto,
    NewProducerDto>(
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
        out NewProducerDto item)
    {
        item = new NewProducerDto
        {
            Name = row.Name,
            Description = row.Description
        };

        return true;
    }

    protected override async Task ProcessBatch(
        IReadOnlyList<(int idx, NewProducerDto item)> producers,
        ProducerImportState state,
        List<CsvImportError> errors)
    {
        if (producers.Count == 0) return;

        var firstIdx = producers[0].idx;

        var result = await sender.Send(
            new CreateProducerBatchCommand(
                producers.Select(x => x.item)),
            CancellationToken);

        foreach (var (idx, message) in result.Errors)
            errors.Add(
                new CsvImportError
                {
                    Message = message,
                    RowIdx = idx >= 0 && idx < producers.Count
                        ? producers[idx].idx
                        : firstIdx + idx
                });

        Logger.LogInformation(
            "Producer import batch processed. JobId: {JobId}, " +
            "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
            "Created: {Created}, Skipped: {Skipped}, Errors: {Errors}",
            JobId,
            firstIdx,
            producers.Count,
            result.Created,
            result.Skipped,
            result.Errors.Count);
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
