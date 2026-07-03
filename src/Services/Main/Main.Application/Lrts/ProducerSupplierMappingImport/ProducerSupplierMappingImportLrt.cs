using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Producer;
using Main.Entities.Producer;
using Main.Enums;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProducerSupplierMappingImport;

public class ProducerSupplierMappingImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    IProducerLookupService producerLookupService,
    ILogger<ProducerSupplierMappingImportLrt> logger,
    IPublishEndpoint publisher,
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions
) : CsvImportLrtBase<
        ProducerSupplierMappingImportState,
        ProducerSupplierMappingImportError,
        ProducerSupplierMappingImportLrt.ProducerSupplierMappingCsvDto,
        ProducerSupplierMappingImportLrt.ProducerSupplierMappingBatchItem>(
        jobRepository,
        unitOfWork,
        publisher,
        logger,
        s3Service,
        stringLocalizer,
        localesOptions)
{
    private ProducerLookup _producerLookup = ProducerLookup.Empty;

    public override string SystemName => nameof(ProducerSupplierMappingImportLrt);
    public override string NameLocalizationKey => "lrt.producer.supplier.mapping.import.name";
    public override string DescriptionLocalizationKey => "lrt.producer.supplier.mapping.import.description";
    public override Type InputType => typeof(ProducerSupplierMappingImportInputState);
    public override Type StateType => typeof(ProducerSupplierMappingImportState);

    protected override async Task BeforeRead(ProducerSupplierMappingImportState state)
    {
        _producerLookup = await producerLookupService.Load(CancellationToken);
    }

    protected override string GetFileName(ProducerSupplierMappingImportState state) => state.FileName;

    protected override int GetCurrentLine(ProducerSupplierMappingImportState state) => state.CurrentLine;

    protected override List<ProducerSupplierMappingImportError> GetErrors(
        ProducerSupplierMappingImportState state) => state.Errors;

    protected override string GetTooManyErrorsLocalizationKey()
        => "producer.too.many.errors.while.processing.batch";

    protected override ProducerSupplierMappingImportError CreateError(int rowIdx, string message)
    {
        return new ProducerSupplierMappingImportError
        {
            RowIdx = rowIdx,
            Message = message
        };
    }

    protected override ProducerSupplierMappingImportState WithUpdatedState(
        ProducerSupplierMappingImportState state,
        int currentLine,
        List<ProducerSupplierMappingImportError> errors)
    {
        return state with
        {
            CurrentLine = currentLine,
            Errors = errors
        };
    }

    protected override bool TryProcessRow(
        int rowIdx,
        ProducerSupplierMappingCsvDto row,
        ProducerSupplierMappingImportState state,
        List<ProducerSupplierMappingImportError> errors,
        out ProducerSupplierMappingBatchItem item)
    {
        item = null!;

        try
        {
            item = new ProducerSupplierMappingBatchItem(
                Producer.Create(row.Producer).Name,
                row.Supplier,
                row.SupplierProducer);
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
        List<(int idx, ProducerSupplierMappingBatchItem item)> mappings,
        ProducerSupplierMappingImportState state,
        List<ProducerSupplierMappingImportError> errors)
    {
        if (mappings.Count == 0) return;

        var firstIdx = mappings[0].idx;
        var items = new List<(int idx, NewProducerSupplierMapping item)>();
        foreach (var (idx, item) in mappings)
        {
            var producerId = _producerLookup.ResolveId(item.ProducerName);
            if (producerId is null)
            {
                errors.Add(CreateError(
                    idx,
                    StringLocalizer.Get("producer.supplier.mapping.producer.not.found.in.batch")));
                continue;
            }

            items.Add((
                idx,
                new NewProducerSupplierMapping
                {
                    ProducerId = producerId.Value,
                    Supplier = item.Supplier,
                    SupplierProducerName = item.SupplierProducerName
                }));
        }

        if (items.Count == 0)
        {
            Logger.LogInformation(
                "Producer supplier mapping import batch skipped. JobId: {JobId}, " +
                "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}",
                JobId,
                firstIdx,
                mappings.Count);

            mappings.Clear();
            return;
        }

        var result = await sender.Send(
            new CreateProducerSupplierMappingBatchCommand(items.Select(x => x.item)),
            CancellationToken);

        foreach (var (idx, message) in result.Errors)
            errors.Add(
                new ProducerSupplierMappingImportError
                {
                    Message = message,
                    RowIdx = idx >= 0 && idx < items.Count
                        ? items[idx].idx
                        : firstIdx + idx
                });

        Logger.LogInformation(
            "Producer supplier mapping import batch processed. JobId: {JobId}, " +
            "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
            "Created: {Created}, Skipped: {Skipped}, Errors: {Errors}",
            JobId,
            firstIdx,
            mappings.Count,
            result.Created,
            result.Skipped,
            result.Errors.Count);

        mappings.Clear();
    }

    public record ProducerSupplierMappingBatchItem(
        string ProducerName,
        Supplier Supplier,
        string SupplierProducerName);

    public record ProducerSupplierMappingCsvDto
    {
        [Name("ProducerName", "Producer")]
        public required string Producer { get; init; }

        [Name("SupplierName", "Supplier")]
        public required Supplier Supplier { get; init; }

        [Name("SupplierProducerName", "SupplierProducer")]
        public required string SupplierProducer { get; init; }
    }
}
