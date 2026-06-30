using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Main.Application.Handlers.Producers;
using Main.Application.Lrts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProducerOtherNamesImport;

public class ProducerOtherNamesImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    ILogger<ProducerOtherNamesImportLrt> logger,
    IPublishEndpoint publisher,
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions)
    : CsvImportLrtBase<
        ProducerOtherNamesImportState,
        ProducerOtherNamesImportError,
        ProducerOtherNamesImportLrt.ProducerOtherNameCsvDto,
        CreateProducerOtherNamesBatchItem>(
        jobRepository,
        unitOfWork,
        publisher,
        logger,
        s3Service,
        stringLocalizer,
        localesOptions)
{
    public override string SystemName => nameof(ProducerOtherNamesImportLrt);
    public override string NameLocalizationKey => "lrt.producer.other.names.import.name";
    public override string DescriptionLocalizationKey => "lrt.producer.other.names.import.description";
    public override Type InputType => typeof(ProducerOtherNamesImportInputState);
    public override Type StateType => typeof(ProducerOtherNamesImportState);

    protected override string GetFileName(ProducerOtherNamesImportState state) => state.FileName;
    protected override int GetCurrentLine(ProducerOtherNamesImportState state) => state.CurrentLine;
    protected override List<ProducerOtherNamesImportError> GetErrors(ProducerOtherNamesImportState state) => state.Errors;
    protected override string GetTooManyErrorsLocalizationKey() => "producer.too.many.errors.while.processing.batch";

    protected override ProducerOtherNamesImportError CreateError(int rowIdx, string message)
    {
        return new ProducerOtherNamesImportError
        {
            RowIdx = rowIdx,
            Message = message
        };
    }

    protected override ProducerOtherNamesImportState WithUpdatedState(
        ProducerOtherNamesImportState state,
        int currentLine,
        List<ProducerOtherNamesImportError> errors)
    {
        return state with
        {
            CurrentLine = currentLine,
            Errors = errors
        };
    }

    protected override bool TryProcessRow(
        int rowIdx,
        ProducerOtherNameCsvDto row,
        ProducerOtherNamesImportState state,
        List<ProducerOtherNamesImportError> errors,
        out CreateProducerOtherNamesBatchItem item)
    {
        item = new CreateProducerOtherNamesBatchItem(
            OriginalName: row.Name,
            OtherName: row.OtherName,
            WhereUsed: row.WhereUsed);
        return true;
    }

    protected override async Task ProcessBatch(
        List<(int idx, CreateProducerOtherNamesBatchItem item)> otherNames,
        ProducerOtherNamesImportState state,
        List<ProducerOtherNamesImportError> errors)
    {
        if (otherNames.Count == 0) return;

        var firstIdx = otherNames[0].idx;
        var result = await sender.Send(
            new CreateProducerOtherNamesBatchCommand(otherNames.Select(x => x.item)),
            CancellationToken);

        foreach (var (idx, message) in result.Errors)
        {
            errors.Add(new ProducerOtherNamesImportError
            {
                Message = message,
                RowIdx = idx >= 0 && idx < otherNames.Count
                    ? otherNames[idx].idx
                    : firstIdx + idx
            });
        }
        
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
    
    public record ProducerOtherNameCsvDto
    {
        [Name("OriginalName", "Name")]
        public required string Name { get; init; }

        [Name("OtherName", "Alias")]
        public required string OtherName { get; init; }
        
        [Name("WhereUsed", "WhereUsed")]
        [Optional]
        public string? WhereUsed { get; init; }
    }
}
