using GreenDonut;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.ProductContent;
using Main.Application.Handlers.Products;
using Main.Application.Handlers.Products.GetProductCrosses;
using Main.Application.Handlers.ProductSizes;
using Main.Application.Handlers.ProductWeight;
using Main.Application.Handlers.StorageContents.GetProductStorageContents;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class ProductDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<int, ProductDto>>
        GetProductByIdAsync(
            IReadOnlyList<int> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProductByIdsQuery(keys),
                cancellationToken))
            .Products
            .ToDictionary(
                x => x.Id,
                x => x);
    }

    [DataLoader]
    public static async Task<Dictionary<int, ProductDto>>
        GetProductPairByIdAsync(
            IReadOnlyList<int> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProductsPairsQuery(keys),
                cancellationToken))
            .Pairs
            .ToDictionary(
                x => x.Key,
                x => x.Value);
    }

    [DataLoader]
    public static async Task<Dictionary<int, ProductSizeDto>>
        GetProductSizeByIdAsync(
            IReadOnlyList<int> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProductsSizesByIdsQuery(keys),
                cancellationToken))
            .Sizes
            .ToDictionary(
                x => x.ProductId,
                x => x);
    }

    [DataLoader]
    public static async Task<Dictionary<int, ProductWeightDto>>
        GetProductWeightByIdAsync(
            IReadOnlyList<int> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProductsWeightsByIdsQuery(keys),
                cancellationToken))
            .Weights
            .ToDictionary(
                x => x.ProductId,
                x => x);
    }
    
    [DataLoader]
    public static async Task<Dictionary<int, List<ProductContentDto>>>
        GetProductContentsByIdAsync(
            IReadOnlyList<int> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProductsContentsQuery(keys),
                cancellationToken))
            .Contents;
    }

    [DataLoader]
    public static async Task<Dictionary<GetProductCrossesItem, IReadOnlyList<ProductDto>>>
        GetProductCrossesAsync(
            IReadOnlyList<GetProductCrossesItem> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProductCrossesQuery(keys),
                cancellationToken))
            .Crosses
            .ToDictionary(
                x => x.Key,
                x => x.Value);
    }

    [DataLoader]
    public static async Task<Dictionary<GetProductStorageContentsItem, IReadOnlyList<StorageContentDto>>>
        GetProductStorageContentsAsync(
            IReadOnlyList<GetProductStorageContentsItem> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProductStorageContentsQuery(keys),
                cancellationToken))
            .Content
            .ToDictionary(
                x => x.Key,
                x => x.Value);
    }
}
