using System.Globalization;
using Abstractions;
using Abstractions.Interfaces.Exceptions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.CreateProducts;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Main.Enums.Products;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Lrts.ProductImport;

public class ProductImportLrt(
    IRepository<Job, Guid> jobRepository,
    IReadRepository<Producer, int> producerReadRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IS3StorageService s3Service,
    ISender sender,
    IPublishEndpoint publisher,
    ILogger<ProductImportLrt> logger,
    IScopedStringLocalizer stringLocalizer,
    IOptions<LocalesOptions> localesOptions) : LrtNamedObjectBase(jobRepository, unitOfWork, publisher, logger)
{
    private const int BatchSize = 100;
    private const int MaxErrors = 10_000;

    public override Type InputType => typeof(ProductImportInputState);
    public override Type StateType => typeof(ProductImportState);
    public override string SystemName => nameof(ProductImportLrt);
    public override string NameLocalizationKey => "lrt.product.import.name";
    public override string DescriptionLocalizationKey => "lrt.product.import.description";

    protected override IServiceDefinition ServiceDefinition => ServicesDefinitions.Main;
    
    private readonly Dictionary<string, int> _producerNamesToIds = new();
    private readonly Dictionary<string, int> _otherNamesToIds = new();

    protected override async Task DoWork()
    {
        stringLocalizer.SetLocale(localesOptions.Value.Default);
        var state = await GetStateAsync<ProductImportState>()
                    ?? throw new InvalidOperationException("Product import state is empty.");

        await using var stream = await s3Service.DownloadFileAsync(
            BucketNames.Uploads,
            state.FileName,
            CancellationToken);

        await LoadProducers();

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var rowIdx = 1;
        var errors = state.Errors;
        var skippedLines = state.SkippedLines;
        var rowsToAdd = new List<(int idx, CreateProductDto product)>();

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            if (rowIdx <= state.CurrentLine)
            {
                rowIdx++;
                continue;
            }

            if (errors.Count >= MaxErrors)
            {
                await UpdateState(state with
                {
                    CurrentLine = rowIdx,
                    Errors = errors,
                    SkippedLines = skippedLines
                });

                Interrupt(stringLocalizer.Get("article.import.too.many.errors.while.processing.batch"));
            }

            NewProductCsvDto row;
            try
            {
                row = csv.GetRecord<NewProductCsvDto>();
            }
            catch (Exception ex) when (ex is CsvHelperException or TypeConverterException)
            {
                errors.Add(new ProductImportError
                {
                    RowIdx = rowIdx,
                    Message = ex.Message
                });

                rowIdx++;
                continue;
            }

            var product = ProcessDto(rowIdx, row, errors);

            if (product != null)
                rowsToAdd.Add((rowIdx, product));

            if (rowsToAdd.Count >= BatchSize)
            {
                await InsertAndClear(rowsToAdd, skippedLines);
                await UpdateState(state with
                {
                    CurrentLine = rowIdx,
                    Errors = errors,
                    SkippedLines = skippedLines
                });
            }

            rowIdx++;
        }

        if (rowsToAdd.Count > 0)
            await InsertAndClear(rowsToAdd, skippedLines);

        await UpdateState(state with
        {
            CurrentLine = rowIdx - 1,
            Errors = errors,
            SkippedLines = skippedLines
        });
    }

    private async Task LoadProducers()
    {
        _producerNamesToIds.Clear();
        _otherNamesToIds.Clear();

        const int batchSize = 1000;
        
        var baseQuery = producerReadRepository.Query
            .Select(x => new
            {
                id = x.Id, 
                name = x.Name,
                otherNames = x.OtherNames.Select(z => z.OtherName)
            })
            .OrderBy(x => x.id);
        
        int lastId = 0;
        
        while (true)
        {
            var id = lastId;
            var producers = await baseQuery
                .Where(x => x.id > id)
                .Take(batchSize)
                .ToListAsync(CancellationToken);

            if (producers.Count == 0) break;

            lastId = producers.Last().id;
            
            foreach (var item in producers)
            {
                _producerNamesToIds.TryAdd(item.name, item.id);
                foreach (var otherName in item.otherNames)
                    _otherNamesToIds.TryAdd(otherName, item.id);
            }
            
            if (producers.Count != batchSize) break;
        }
    }

    private CreateProductDto? ProcessDto(
        int idx,
        NewProductCsvDto row,
        List<ProductImportError> errors)
    {
        try
        {
            var producerId = ResolveProducerId(row.Producer);
            if (producerId == null)
            {
                errors.Add(new ProductImportError
                {
                    RowIdx = idx,
                    Message = string.Format(
                        stringLocalizer.Get("article.import.producer.not.found"),
                        row.Producer)
                });

                return null;
            }

            var product = Product.Create(row.Sku, row.Name, producerId.Value, row.Description);
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
            errors.Add(new ProductImportError
            {
                RowIdx = idx,
                Message = GetErrorMessage(ex)
            });

            return null;
        }
    }

    private int? ResolveProducerId(string producer)
    {
        if (string.IsNullOrWhiteSpace(producer))
            return null;

        var normalizedProducer = Producer.ToNormalizedName(producer);

        if (_producerNamesToIds.TryGetValue(normalizedProducer, out var producerId))
            return producerId;

        return _otherNamesToIds.TryGetValue(normalizedProducer, out var otherNameProducerId)
            ? otherNameProducerId
            : null;
    }

    private async Task InsertAndClear(
        List<(int idx, CreateProductDto product)> products,
        List<int> skippedLines)
    {
        if (products.Count == 0) return;

        var firstIdx = products[0].idx;
        var toCreate = await FilterExistingAndDuplicateProducts(products, skippedLines);

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
                toCreate.Select(x => x.product).ToList(),
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

    private async Task<List<(int idx, CreateProductDto product)>> FilterExistingAndDuplicateProducts(
        List<(int idx, CreateProductDto product)> products,
        List<int> skippedLines)
    {
        var keys = products
            .Select(x => GetProductKey(x.product))
            .ToList();

        var existingKeys = await productRepository.GetExistingProductKeys(keys, CancellationToken);
        var currentBatchKeys = new HashSet<(string NormalizedSku, int ProducerId)>();
        var toCreate = new List<(int idx, CreateProductDto product)>();

        foreach (var item in products)
        {
            var key = GetProductKey(item.product);

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
            return stringLocalizer.GetOrDefault(localizableException.MessageKey) ?? ex.Message;

        return ex.Message;
    }
    
    private record NewProductCsvDto
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
