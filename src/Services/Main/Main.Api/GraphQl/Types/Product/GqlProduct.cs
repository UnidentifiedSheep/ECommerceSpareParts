using Enums;
using HotChocolate;
using HotChocolate.Types.Composite;
using GraphQL.Common.Attributes;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products;
using Main.Application.Handlers.Products.GetProductCrosses;
using Main.Application.Handlers.StorageContents.GetProductStorageContents;
using Main.Entities.Exceptions;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("Product")]
public record GqlProduct
{
    private readonly ProductDto? _product;

    [GraphQLName("id")]
    [Shareable]
    public int Id { get; }

    [GraphQLName("sku")]
    public async Task<string> GetSkuAsync(
        IProductByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await GetProductAsync(loader, cancellationToken)).Sku;

    [GraphQLName("name")]
    public async Task<string> GetNameAsync(
        IProductByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await GetProductAsync(loader, cancellationToken)).Name;

    [GraphQLName("description")]
    public async Task<string?> GetDescriptionAsync(
        IProductByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await GetProductAsync(loader, cancellationToken)).Description;

    [GraphQLName("indicator")]
    public async Task<string?> GetIndicatorAsync(
        IProductByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await GetProductAsync(loader, cancellationToken)).Indicator;

    [GraphQLName("images")]
    public async Task<List<string>> GetImagesAsync(
        IProductByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await GetProductAsync(loader, cancellationToken)).Images;

    [GraphQLName("stock")]
    public async Task<int> GetStockAsync(
        IProductByIdDataLoader loader,
        CancellationToken cancellationToken)
        => (await GetProductAsync(loader, cancellationToken)).Stock;

    [GraphQLName("producer")]
    public async Task<GqlProducer> GetProducerAsync(
        IProductByIdDataLoader productLoader,
        CancellationToken cancellationToken)
    {
        var product = await GetProductAsync(productLoader, cancellationToken);
        return new GqlProducer(product.ProducerId);
    }

    [GraphQLName("size")]
    public async Task<GqlProductSize?> GetSizeAsync(
        IProductSizeByIdDataLoader loader,
        CancellationToken cancellationToken)
    {
        var size = await loader.LoadAsync(Id, cancellationToken);
        return size is null ? null : new GqlProductSize(size);
    }

    [GraphQLName("weight")]
    public async Task<GqlProductWeight?> GetWeightAsync(
        IProductWeightByIdDataLoader loader,
        CancellationToken cancellationToken)
    {
        var weight = await loader.LoadAsync(Id, cancellationToken);
        return weight is null ? null : new GqlProductWeight(weight);
    }

    [GraphQLName("pair")]
    public async Task<GqlProduct?> GetPairAsync(
        IProductPairByIdDataLoader loader,
        CancellationToken cancellationToken)
    {
        var pair = await loader.LoadAsync(Id, cancellationToken);
        return pair is null ? null : new GqlProduct(pair);
    }
    
    [GraphQLName("contents")]
    public async Task<IReadOnlyList<GqlProductContent>> GetContentsAsync(
        IProductContentsByIdDataLoader loader,
        CancellationToken cancellationToken)
    {
        var contents = await loader.LoadAsync(Id, cancellationToken);
        return contents?
            .Select(x => new GqlProductContent(x))
            .ToArray() ?? [];
    }

    [GraphQLName("crosses")]
    public async Task<IReadOnlyList<GqlProduct>> GetCrossesAsync(
        GqlProductCrossesInput input,
        IProductCrossesDataLoader loader,
        CancellationToken cancellationToken)
    {
        var item = new GetProductCrossesItem(
            Id,
            input.Pagination,
            input.SortBy?
                .Select(x => x.ToSortExpression())
                .ToArray());

        var crosses = await loader.LoadAsync(item, cancellationToken);
        return crosses?
            .Select(x => new GqlProduct(x))
            .ToArray() ?? [];
    }

    [GraphQLName("storageContents")]
    [RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_GET_ALL)]
    public async Task<IReadOnlyList<GqlStorageContent>> GetStorageContentsAsync(
        GqlProductStorageContentsInput input,
        IProductStorageContentsDataLoader loader,
        CancellationToken cancellationToken)
    {
        var item = new GetProductStorageContentsItem(
            Id,
            input.Pagination,
            input.StorageCode,
            input.ShowZeroCount);

        var content = await loader.LoadAsync(item, cancellationToken);
        return content?
            .Select(x => new GqlStorageContent(x))
            .ToArray() ?? [];
    }

    [GraphQLName("availableStock")]
    public async Task<int> GetAvailableStockAsync(
        IProductAvailableStockDataLoader loader,
        GqlProductAvailableStocksInput input,
        CancellationToken cancellationToken)
        => await loader.LoadAsync(
            new GetAvailableProductsStockItem(Id, input.StorageCode),
            cancellationToken);

    private async Task<ProductDto> GetProductAsync(
        IProductByIdDataLoader loader,
        CancellationToken cancellationToken)
        => _product
           ?? await loader.LoadAsync(Id, cancellationToken)
           ?? throw new ProductNotFoundException(Id);

    public GqlProduct(int id)
    {
        Id = id;
    }

    public GqlProduct(ProductDto product) : this(product.Id)
    {
        _product = product;
    }
}
