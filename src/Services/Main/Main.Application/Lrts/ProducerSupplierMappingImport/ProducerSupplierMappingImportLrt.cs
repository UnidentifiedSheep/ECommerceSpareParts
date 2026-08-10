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
using Domain.Extensions;
using Enums;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Lrts.Base;
using Main.Application.Models.Producer;
using Main.Entities.Producer;
using Main.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProducerSupplierMappingImport;

public class ProducerSupplierMappingImportLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    IProducerRepository producerRepository,
    IProducerLookupService producerLookupService,
    ILogger<ProducerSupplierMappingImportLrt> logger,
    IOptions<S3BucketsOptions> bucketsOptions,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    IScopedStringLocalizer stringLocalizer
) : CsvImportLrtBase<
        ProducerSupplierMappingImportInputState,
        ProducerSupplierMappingImportState,
        ProducerSupplierMappingImportLrt.ProducerSupplierMappingCsvDto,
        ProducerSupplierMappingImportLrt.ProducerSupplierMappingBatchItem>(
        jobRepository,
        bucketsOptions,
        unitOfWork,
        publisher,
        transactionService,
        logger,
        s3Service,
        stringLocalizer)
{
    private IProducerLookup _producerLookup = ProducerLookup.Empty;

    public override string SystemName => nameof(ProducerSupplierMappingImportLrt);
    public override string NameLocalizationKey => "lrt.producer.supplier.mapping.import.name";
    public override string DescriptionLocalizationKey => "lrt.producer.supplier.mapping.import.description";

    protected override async Task BeforeRead(ProducerSupplierMappingImportState state)
    {
        _producerLookup = await producerLookupService.Load(CancellationToken);
    }

    protected override string GetTooManyErrorsLocalizationKey()
        => "producer.too.many.errors.while.processing.batch";

    protected override bool TryProcessRow(
        int rowIdx,
        ProducerSupplierMappingCsvDto row,
        ProducerSupplierMappingImportState state,
        List<CsvImportError> errors,
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
        IReadOnlyList<(int idx, ProducerSupplierMappingBatchItem item)> mappings,
        ProducerSupplierMappingImportState state,
        List<CsvImportError> errors)
    {
        if (mappings.Count == 0) return;

        var firstIdx = mappings[0].idx;
        var errorsBeforeBatch = errors.Count;
        var toAdd = new List<ProducerSupplierMapping>();
        var uniqueMappings = new HashSet<(
            string SupplierProducerName,
            Supplier Supplier)>();

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

            var supplierProducerName = item.SupplierProducerName.TrimSafe();
            if (string.IsNullOrWhiteSpace(supplierProducerName))
            {
                errors.Add(CreateError(
                    idx,
                    StringLocalizer.Get(
                        "producer.supplier.mapping.supplier.producer.name.required")));
                continue;
            }

            if (!uniqueMappings.Add((supplierProducerName, item.Supplier)))
            {
                errors.Add(CreateError(
                    idx,
                    StringLocalizer.Get(
                        "producer.supplier.mapping.duplicate.in.batch")));
                continue;
            }

            toAdd.Add(
                ProducerSupplierMapping.Create(
                    producerId.Value,
                    supplierProducerName,
                    item.Supplier));
        }

        if (toAdd.Count == 0)
        {
            Logger.LogInformation(
                "Producer supplier mapping import batch skipped. JobId: {JobId}, " +
                "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}",
                JobId,
                firstIdx,
                mappings.Count);

            return;
        }

        await TransactionService.ExecuteAsync(
            TransactionalAttribute.RetryOnConflict(20, 2),
            (_, cancellationToken) =>
                producerRepository.AddSupplierMappingsOnConflictDoNothingAsync(
                    toAdd,
                    cancellationToken),
            CancellationToken);

        Logger.LogInformation(
            "Producer supplier mapping import batch processed. JobId: {JobId}, " +
            "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
            "Created: {Created}, Skipped: {Skipped}, Errors: {Errors}",
            JobId,
            firstIdx,
            mappings.Count,
            toAdd.Count,
            mappings.Count - toAdd.Count,
            errors.Count - errorsBeforeBatch);
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
