using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
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
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions
)
    : CsvImportLrtBase<
        ProducerAliasesImportState,
        ProducerAliasesImportError,
        ProducerAliasImportLrt.ProducerAliasCsvDto,
        CreateProducerAliasesBatchItem>(
        jobRepository,
        unitOfWork,
        publisher,
        logger,
        s3Service,
        stringLocalizer,
        localesOptions)
{
    public override string SystemName => nameof(ProducerAliasImportLrt);
    public override string NameLocalizationKey => "lrt.producer.other.names.import.name";
    public override string DescriptionLocalizationKey => "lrt.producer.other.names.import.description";
    public override Type InputType => typeof(ProducerAliasesImportInputState);
    public override Type StateType => typeof(ProducerAliasesImportState);

    protected override string GetFileName(ProducerAliasesImportState state) { return state.FileName; }

    protected override int GetCurrentLine(ProducerAliasesImportState state) { return state.CurrentLine; }

    protected override List<ProducerAliasesImportError> GetErrors(ProducerAliasesImportState state)
    {
        return state.Errors;
    }

    protected override string GetTooManyErrorsLocalizationKey()
    {
        return "producer.too.many.errors.while.processing.batch";
    }

    protected override ProducerAliasesImportError CreateError(int rowIdx, string message)
    {
        return new ProducerAliasesImportError
        {
            RowIdx = rowIdx,
            Message = message
        };
    }

    protected override ProducerAliasesImportState WithUpdatedState(
        ProducerAliasesImportState state,
        int currentLine,
        List<ProducerAliasesImportError> errors)
    {
        return state with
        {
            CurrentLine = currentLine,
            Errors = errors
        };
    }

    protected override bool TryProcessRow(
        int rowIdx,
        ProducerAliasCsvDto row,
        ProducerAliasesImportState state,
        List<ProducerAliasesImportError> errors,
        out CreateProducerAliasesBatchItem item)
    {
        item = new CreateProducerAliasesBatchItem(
            row.Name,
            row.Alias);
        return true;
    }

    protected override async Task ProcessBatch(
        List<(int idx, CreateProducerAliasesBatchItem item)> otherNames,
        ProducerAliasesImportState state,
        List<ProducerAliasesImportError> errors)
    {
        if (otherNames.Count == 0) return;

        var firstIdx = otherNames[0].idx;
        var result = await sender.Send(
            new CreateProducerAliasesBatchCommand(otherNames.Select(x => x.item)),
            CancellationToken);

        foreach (var (idx, message) in result.Errors)
            errors.Add(
                new ProducerAliasesImportError
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

        otherNames.Clear();
    }

    public record ProducerAliasCsvDto
    {
        [Name("OriginalName", "Name")]
        public required string Name { get; init; }

        [Name("OtherName", "Alias")]
        public required string Alias { get; init; }
    }
}