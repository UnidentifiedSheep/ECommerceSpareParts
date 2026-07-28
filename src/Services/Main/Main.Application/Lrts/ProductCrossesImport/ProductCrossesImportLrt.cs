using Abstractions.Interfaces;
using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Main.Application.Handlers.Products.UpsertProductCrosses;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Lrts.Base;
using Main.Application.Models.Producer;
using Main.Entities.Product.ValueObjects;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProductCrossesImport;

public class ProductCrossesImportLrt(
    IRepository<Job, Guid> jobRepository,
    IProducerLookupService producerLookupService,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    IPublishEndpoint publisher,
    IOptions<S3BucketsOptions> bucketsOptions,
    ILogger<ProductCrossesImportLrt> logger,
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions)
    : CsvImportLrtBase<
        ProductCrossesImportState,
        ProductCrossesImportError,
        ProductCrossesImportLrt.ProductCrossCsvDto,
        ProductCrossesImportLrt.ProductCrossBatchItem>(
        jobRepository,
        bucketsOptions,
        unitOfWork,
        publisher,
        logger,
        s3Service,
        stringLocalizer,
        localesOptions)
{
    private ProducerLookup _producerLookup = ProducerLookup.Empty;

    public override string SystemName => nameof(ProductCrossesImportLrt);
    public override string NameLocalizationKey => "lrt.product.crosses.import.name";
    public override string DescriptionLocalizationKey => "lrt.product.crosses.import.description";
    public override Type InputType => typeof(ProductCrossesImportInputState);
    public override Type StateType => typeof(ProductCrossesImportState);

    protected override async Task BeforeRead(ProductCrossesImportState state)
    {
        _producerLookup = await producerLookupService.Load(CancellationToken);
    }

    protected override string GetFileName(ProductCrossesImportState state) => state.FileName;

    protected override int GetCurrentLine(ProductCrossesImportState state) => state.CurrentLine;

    protected override List<ProductCrossesImportError> GetErrors(ProductCrossesImportState state)
        => state.Errors;

    protected override ProductCrossesImportError CreateError(int rowIdx, string message)
    {
        return new ProductCrossesImportError
        {
            RowIdx = rowIdx,
            Message = message
        };
    }

    protected override string GetTooManyErrorsLocalizationKey()
        => "article.import.too.many.errors.while.processing.batch";

    protected override ProductCrossesImportState WithUpdatedState(
        ProductCrossesImportState state,
        int currentLine,
        List<ProductCrossesImportError> errors)
    {
        return state with
        {
            CurrentLine = currentLine,
            Errors = errors
        };
    }

    protected override bool TryProcessRow(
        int rowIdx,
        ProductCrossCsvDto row,
        ProductCrossesImportState state,
        List<ProductCrossesImportError> errors,
        out ProductCrossBatchItem item)
    {
        item = null!;

        try
        {
            var producerId = _producerLookup.ResolveId(row.Producer);
            if (producerId is null)
            {
                errors.Add(CreateError(
                    rowIdx,
                    StringLocalizer.Get("article.import.producer.not.found", row.Producer)));
                state.SkippedLines.Add(rowIdx);
                return false;
            }

            var crossProducerId = _producerLookup.ResolveId(row.CrossProducer);
            if (crossProducerId is null)
            {
                errors.Add(CreateError(
                    rowIdx,
                    StringLocalizer.Get("article.import.producer.not.found", row.CrossProducer)));
                state.SkippedLines.Add(rowIdx);
                return false;
            }

            item = new ProductCrossBatchItem(
                new ProductKey(new Sku(row.Sku).NormalizedValue, producerId.Value),
                new ProductKey(new Sku(row.CrossSku).NormalizedValue, crossProducerId.Value),
                row.Sku,
                row.Producer,
                row.CrossSku,
                row.CrossProducer);

            return true;
        }
        catch (Exception ex)
        {
            errors.Add(CreateError(rowIdx, GetErrorMessage(ex)));
            state.SkippedLines.Add(rowIdx);
            return false;
        }
    }

    protected override async Task ProcessBatch(
        List<(int idx, ProductCrossBatchItem item)> items,
        ProductCrossesImportState state,
        List<ProductCrossesImportError> errors)
    {
        if (items.Count == 0) return;

        var firstIdx = items[0].idx;
        var errorsBeforeBatch = errors.Count;
        var productIds = await productRepository.GetProductIdsByKeys(
            items.SelectMany(x => new[]
            {
                x.item.Product.ToTuple(),
                x.item.CrossProduct.ToTuple()
            }),
            CancellationToken);

        var crosses = new HashSet<(int ProductId, int CrossProductId)>();

        foreach (var (idx, item) in items)
        {
            if (!productIds.TryGetValue(item.Product.ToTuple(), out var productId))
            {
                errors.Add(CreateError(
                    idx,
                    StringLocalizer.Get(
                        "article.cross.import.product.not.found",
                        item.Sku,
                        item.Producer)));
                state.SkippedLines.Add(idx);
                continue;
            }

            if (!productIds.TryGetValue(item.CrossProduct.ToTuple(), out var crossProductId))
            {
                errors.Add(CreateError(
                    idx,
                    StringLocalizer.Get(
                        "article.cross.import.product.not.found",
                        item.CrossSku,
                        item.CrossProducer)));
                state.SkippedLines.Add(idx);
                continue;
            }

            if (productId == crossProductId)
            {
                errors.Add(CreateError(
                    idx,
                    StringLocalizer.Get("article.linkage.article.cannot.equal.cross.article")));
                state.SkippedLines.Add(idx);
                continue;
            }

            var cross = productId < crossProductId
                ? (productId, crossProductId)
                : (crossProductId, productId);

            if (!crosses.Add(cross))
                state.SkippedLines.Add(idx);
        }

        if (crosses.Count > 0)
            await sender.Send(
                new UpsertProductCrossesCommand(crosses),
                CancellationToken);

        Logger.LogInformation(
            "Product crosses import batch processed. JobId: {JobId}, " +
            "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
            "Upserted: {Upserted}, Errors: {Errors}",
            JobId,
            firstIdx,
            items.Count,
            crosses.Count,
            errors.Count - errorsBeforeBatch);

        items.Clear();
    }

    private string GetErrorMessage(Exception ex)
    {
        if (ex is ILocalizableException localizableException)
            return StringLocalizer.GetOrDefault(
                localizableException.MessageKey,
                localizableException.Arguments ?? []) ?? ex.Message;

        return ex.Message;
    }

    public sealed record ProductKey(string NormalizedSku, int ProducerId)
    {
        public (string NormalizedSku, int ProducerId) ToTuple()
            => (NormalizedSku, ProducerId);
    }

    public sealed record ProductCrossBatchItem(
        ProductKey Product,
        ProductKey CrossProduct,
        string Sku,
        string Producer,
        string CrossSku,
        string CrossProducer);

    public sealed record ProductCrossCsvDto
    {
        [Name("Sku")]
        public required string Sku { get; init; }

        [Name("Producer")]
        public required string Producer { get; init; }

        [Name("CrossSku")]
        public required string CrossSku { get; init; }

        [Name("CrossProducer")]
        public required string CrossProducer { get; init; }
    }
}
