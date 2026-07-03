using Abstractions.Interfaces;
using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using CsvHelper.Configuration.Attributes;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.CreateProducts;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Producer;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Main.Enums.Products;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProductImport;

public class ProductImportLrt(
    IRepository<Job, Guid> jobRepository,
    IProducerLookupService producerLookupService,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    IPublishEndpoint publisher,
    ILogger<ProductImportLrt> logger,
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions
)
    : CsvImportLrtBase<ProductImportState, ProductImportError, ProductImportLrt.NewProductCsvDto,
        CreateProductDto>(
        jobRepository,
        unitOfWork,
        publisher,
        logger,
        s3Service,
        stringLocalizer,
        localesOptions)
{
    private ProducerLookup _producerLookup = ProducerLookup.Empty;

    protected override int BatchSize => 100;
    public override Type InputType => typeof(ProductImportInputState);
    public override Type StateType => typeof(ProductImportState);
    public override string SystemName => nameof(ProductImportLrt);
    public override string NameLocalizationKey => "lrt.product.import.name";
    public override string DescriptionLocalizationKey => "lrt.product.import.description";

    protected override async Task BeforeRead(ProductImportState state)
    {
        _producerLookup = await producerLookupService.Load(CancellationToken);
    }

    protected override string GetFileName(ProductImportState state) { return state.FileName; }

    protected override int GetCurrentLine(ProductImportState state) { return state.CurrentLine; }

    protected override List<ProductImportError> GetErrors(ProductImportState state) { return state.Errors; }

    protected override string GetTooManyErrorsLocalizationKey()
    {
        return "article.import.too.many.errors.while.processing.batch";
    }

    protected override ProductImportError CreateError(int rowIdx, string message)
    {
        return new ProductImportError
        {
            RowIdx = rowIdx,
            Message = message
        };
    }

    protected override ProductImportState WithUpdatedState(
        ProductImportState state,
        int currentLine,
        List<ProductImportError> errors)
    {
        return state with
        {
            CurrentLine = currentLine,
            Errors = errors,
            SkippedLines = state.SkippedLines
        };
    }

    protected override bool TryProcessRow(
        int rowIdx,
        NewProductCsvDto row,
        ProductImportState state,
        List<ProductImportError> errors,
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
        List<ProductImportError> errors)
    {
        try
        {
            var producerId = _producerLookup.ResolveId(row.Producer);
            if (producerId == null)
            {
                errors.Add(
                    new ProductImportError
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
                new ProductImportError
                {
                    RowIdx = idx,
                    Message = GetErrorMessage(ex)
                });

            return null;
        }
    }

    protected override async Task ProcessBatch(
        List<(int idx, CreateProductDto item)> products,
        ProductImportState state,
        List<ProductImportError> errors)
    {
        if (products.Count == 0) return;

        var firstIdx = products[0].idx;
        var toCreate = await FilterExistingAndDuplicateProducts(products, state.SkippedLines);

        if (toCreate.Count == 0)
        {
            Logger.LogInformation(
                "Product import batch skipped. JobId: {JobId}, BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}",
                JobId,
                firstIdx,
                products.Count);

            products.Clear();
            return;
        }

        var result = await sender.Send(
            new CreateProductsCommand(
                toCreate.Select(x => x.item).ToList(),
                CreateProductsConflictPolicy.SkipExisting),
            CancellationToken);

        Logger.LogInformation(
            "Product import batch processed. JobId: {JobId}, " +
            "BatchStartRow: {BatchStartRow}, BatchSize: {BatchSize}, " +
            "Created: {Created}, Skipped: {Skipped}",
            JobId,
            firstIdx,
            products.Count,
            result.CreatedIds.Count,
            products.Count - toCreate.Count + result.Skipped);

        products.Clear();
    }

    private async Task<List<(int idx, CreateProductDto item)>> FilterExistingAndDuplicateProducts(
        List<(int idx, CreateProductDto item)> products,
        List<int> skippedLines)
    {
        var keys = products
            .Select(x => GetProductKey(x.item))
            .ToList();

        var existingKeys = await productRepository.GetExistingProductKeys(keys, CancellationToken);
        var currentBatchKeys = new HashSet<(string NormalizedSku, int ProducerId)>();
        var toCreate = new List<(int idx, CreateProductDto product)>();

        foreach (var item in products)
        {
            var key = GetProductKey(item.item);

            if (existingKeys.Contains(key) || !currentBatchKeys.Add(key))
            {
                skippedLines.Add(item.idx);
                continue;
            }

            toCreate.Add(item);
        }

        return toCreate;
    }

    private static (string NormalizedSku, int ProducerId) GetProductKey(CreateProductDto product)
    {
        return (new Sku(product.Sku).NormalizedValue, product.ProducerId);
    }

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
