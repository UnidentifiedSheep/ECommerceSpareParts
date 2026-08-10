using Abstractions.Interfaces;
using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Lrts.Base;
using Main.Application.Models.Producer;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProductImport;

public class ProductImportLrt(
    IRepository<Job, Guid> jobRepository,
    IProducerLookupService producerLookupService,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    IOptions<S3BucketsOptions> bucketsOptions,
    ILogger<ProductImportLrt> logger,
    IScopedStringLocalizer stringLocalizer
)
    : CsvImportLrtBase<ProductImportInputState, ProductImportState, ProductImportLrt.NewProductCsvDto,
        CreateProductDto>(
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

    protected override int BatchSize => 1000;
    public override string SystemName => nameof(ProductImportLrt);
    public override string NameLocalizationKey => "lrt.product.import.name";
    public override string DescriptionLocalizationKey => "lrt.product.import.description";

    protected override async Task BeforeRead(ProductImportState state)
    {
        _producerLookup = await producerLookupService.Load(CancellationToken);
    }

    protected override string GetTooManyErrorsLocalizationKey()
    {
        return "article.import.too.many.errors.while.processing.batch";
    }

    protected override bool TryProcessRow(
        int rowIdx,
        NewProductCsvDto row,
        ProductImportState state,
        List<CsvImportError> errors,
        out CreateProductDto item)
    {
        var product = ProcessDto(
            rowIdx,
            row,
            errors);
        item = product!;
        return product is not null;
    }

    private CreateProductDto? ProcessDto(
        int idx,
        NewProductCsvDto row,
        List<CsvImportError> errors)
    {
        try
        {
            var producerId = _producerLookup.ResolveId(row.Producer);
            if (producerId == null)
            {
                errors.Add(
                    new CsvImportError
                    {
                        RowIdx = idx,
                        Message = StringLocalizer.Get("article.import.producer.not.found", row.Producer)
                    });

                return null;
            }

            var product = Product.Create(
                row.Sku,
                row.Name,
                producerId.Value,
                row.Description);
            product.SetIndicator(row.Indicator);
            product.SetCategory(row.CategoryId);

            return new CreateProductDto
            {
                Sku = product.Sku.Value,
                Name = product.Name.Value,
                ProducerId = product.ProducerId,
                Description = product.Description,
                Indicator = product.Indicator,
                CategoryId = product.CategoryId
            };
        }
        catch (Exception ex)
        {
            errors.Add(
                new CsvImportError
                {
                    RowIdx = idx,
                    Message = GetErrorMessage(ex)
                });

            return null;
        }
    }

    protected override async Task ProcessBatch(
        IReadOnlyList<(int idx, CreateProductDto item)> products,
        ProductImportState state,
        List<CsvImportError> errors)
    {
        if (products.Count == 0) return;

        var firstIdx = products[0].idx;
        var uniqueKeys = new HashSet<(string NormalizedSku, int ProducerId)>();
        var uniqueProducts = new List<(int idx, CreateProductDto item)>();
        foreach (var product in products)
        {
            if (uniqueKeys.Add(GetProductKey(product.item)))
            {
                uniqueProducts.Add(product);
                continue;
            }

            state.SkippedLines.Add(product.idx);
        }

        if (uniqueProducts.Count == 0)
        {
            Logger.LogInformation(
                "Product import batch skipped. JobId: {JobId}, BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}",
                JobId,
                firstIdx,
                products.Count);

            return;
        }

        var result = await TransactionService.ExecuteAsync(
            TransactionalAttribute.RetryOnConflict(20, 2),
            async (context, cancellationToken) =>
            {
                var existingKeys = await productRepository.GetExistingProductKeys(
                    uniqueKeys,
                    cancellationToken);
                var toCreate = uniqueProducts
                    .Where(x => !existingKeys.Contains(GetProductKey(x.item)))
                    .ToList();
                var entities = toCreate
                    .Select(x => CreateProduct(x.item))
                    .ToList();

                await context.UnitOfWork.AddRangeAsync(entities, cancellationToken);
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);

                return new ProductImportBatchResult(
                    entities.Count,
                    uniqueProducts
                        .Where(x => existingKeys.Contains(GetProductKey(x.item)))
                        .Select(x => x.idx)
                        .ToList());
            },
            CancellationToken);
        state.SkippedLines.AddRange(result.SkippedLines);

        Logger.LogInformation(
            "Product import batch processed. JobId: {JobId}, " +
            "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
            "Created: {Created}, Skipped: {Skipped}",
            JobId,
            firstIdx,
            products.Count,
            result.Created,
            products.Count - result.Created);
    }

    private static Product CreateProduct(CreateProductDto item)
    {
        var product = Product.Create(
            item.Sku,
            item.Name,
            item.ProducerId,
            item.Description);
        product.SetIndicator(item.Indicator);
        product.SetCategory(item.CategoryId);
        return product;
    }

    private static (string NormalizedSku, int ProducerId) GetProductKey(CreateProductDto product)
    {
        return (new Sku(product.Sku).NormalizedValue, product.ProducerId);
    }

    private sealed record ProductImportBatchResult(
        int Created,
        IReadOnlyList<int> SkippedLines);

    private string GetErrorMessage(Exception ex)
    {
        if (ex is ILocalizableException localizableException)
            return StringLocalizer.GetOrDefault(
                localizableException.MessageKey,
                localizableException.Arguments ?? []) ?? ex.Message;

        return ex.Message;
    }

    public record NewProductCsvDto
    {
        [Name("Sku")]
        public required string Sku { get; init; }

        [Name("Name")]
        public required string Name { get; init; }

        [Name("Producer", "ProducerName")]
        public required string Producer { get; init; }

        [Optional]
        [Name("Description")]
        public string? Description { get; init; }

        [Optional]
        [Name("Indicator")]
        public string? Indicator { get; init; }

        [Optional]
        [Name("CategoryId")]
        public int? CategoryId { get; init; }
    }
}
