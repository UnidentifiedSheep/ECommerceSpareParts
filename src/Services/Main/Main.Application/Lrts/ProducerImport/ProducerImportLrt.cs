using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers;
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
    IPublishEndpoint publisher,
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions
)
    : CsvImportLrtBase<ProducerImportState, ProducerImportError, ProducerImportLrt.NewProducerCsvDto,
        NewProducerDto>(
        jobRepository,
        unitOfWork,
        publisher,
        logger,
        s3Service,
        stringLocalizer,
        localesOptions)
{
    public override string SystemName => nameof(ProducerImportLrt);
    public override string NameLocalizationKey => "lrt.producer.import.name";
    public override string DescriptionLocalizationKey => "lrt.producer.import.description";
    public override Type InputType => typeof(ProducerImportInputState);
    public override Type StateType => typeof(ProducerImportState);

    protected override string GetFileName(ProducerImportState state) { return state.FileName; }

    protected override int GetCurrentLine(ProducerImportState state) { return state.CurrentLine; }

    protected override List<ProducerImportError> GetErrors(ProducerImportState state) { return state.Errors; }

    protected override string GetTooManyErrorsLocalizationKey()
    {
        return "producer.too.many.errors.while.processing.batch";
    }

    protected override ProducerImportError CreateError(int rowIdx, string message)
    {
        return new ProducerImportError
        {
            RowIdx = rowIdx,
            Message = message
        };
    }

    protected override ProducerImportState WithUpdatedState(
        ProducerImportState state,
        int currentLine,
        List<ProducerImportError> errors)
    {
        return state with
        {
            CurrentLine = currentLine,
            Errors = errors
        };
    }

    protected override bool TryProcessRow(
        int rowIdx,
        NewProducerCsvDto row,
        ProducerImportState state,
        List<ProducerImportError> errors,
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
        List<(int idx, NewProducerDto item)> producers,
        ProducerImportState state,
        List<ProducerImportError> errors)
    {
        if (producers.Count == 0) return;

        var firstIdx = producers[0].idx;

        var result = await sender.Send(
            new CreateProducerBatchCommand(
                producers.Select(x => x.item)),
            CancellationToken);

        foreach (var (idx, message) in result.Errors)
            errors.Add(
                new ProducerImportError
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

        producers.Clear();
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