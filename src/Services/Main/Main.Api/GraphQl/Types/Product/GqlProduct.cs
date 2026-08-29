using Enums;
using HotChocolate;
using HotChocolate.Types.Composite;
using GraphQL.Common.Attributes;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.GetProductCrosses;
using Main.Application.Handlers.StorageContents.GetProductStorageContents;

namespace Main.Api.GraphQl.Types.Product;

[GraphQLName("Product")]
public record GqlProduct(
    [property: GraphQLIgnore]
    ProductDto Product)
{
    [GraphQLName("id")]
    [Shareable]
    public int Id => Product.Id;

    [GraphQLName("sku")]
    public string Sku => Product.Sku;

    [GraphQLName("name")]
    public string Name => Product.Name;

    [GraphQLName("description")]
    public string? Description => Product.Description;

    [GraphQLName("indicator")]
    public string? Indicator => Product.Indicator;

    [GraphQLName("images")]
    public List<string> Images => Product.Images;

    [GraphQLName("stock")]
    public int Stock => Product.Stock;

    [GraphQLName("producer")]
    public async Task<GqlProducer> GetProducerAsync(
        IProducerByIdDataLoader producerById,
        CancellationToken cancellationToken)
    {
        var producer = await producerById.LoadAsync(
            Product.ProducerId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Producer with id {Product.ProducerId} does not exist");

        return producer;
    }

    [GraphQLName("size")]
    public Task<GqlProductSize?> GetSizeAsync(
        IProductSizeByIdDataLoader loader,
        CancellationToken cancellationToken)
        => loader.LoadAsync(Id, cancellationToken);

    [GraphQLName("weight")]
    public Task<GqlProductWeight?> GetWeightAsync(
        IProductWeightByIdDataLoader loader,
        CancellationToken cancellationToken)
        => loader.LoadAsync(Id, cancellationToken);

    [GraphQLName("pair")]
    public Task<GqlProduct?> GetPairAsync(
        IProductPairByIdDataLoader loader,
        CancellationToken cancellationToken)
        => loader.LoadAsync(Id, cancellationToken);
    
    [GraphQLName("contents")]
    public async Task<IReadOnlyList<GqlProductContent>> GetContentsAsync(
        IProductContentsByIdDataLoader loader,
        CancellationToken cancellationToken)
        => await loader.LoadAsync(Id, cancellationToken) ?? [];

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

        return await loader.LoadAsync(item, cancellationToken) ?? [];
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

        return await loader.LoadAsync(item, cancellationToken) ?? [];
    }
}
