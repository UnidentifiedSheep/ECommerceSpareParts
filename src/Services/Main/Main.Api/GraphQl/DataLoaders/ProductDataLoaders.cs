using GreenDonut;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Handlers.ProductContent;
using Main.Application.Handlers.Products;
using Main.Application.Handlers.Products.GetProductCrosses;
using Main.Application.Handlers.ProductSizes;
using Main.Application.Handlers.ProductWeight;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class ProductDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<int, GqlProduct>>
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
                x => new GqlProduct(x));
    }

    [DataLoader]
    public static async Task<Dictionary<int, GqlProduct>>
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
                x => new GqlProduct(x.Value));
    }

    [DataLoader]
    public static async Task<Dictionary<int, GqlProductSize>>
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
                x => new GqlProductSize(x));
    }

    [DataLoader]
    public static async Task<Dictionary<int, GqlProductWeight>>
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
                x => new GqlProductWeight(x));
    }
    
    [DataLoader]
    public static async Task<Dictionary<int, List<GqlProductContent>>>
        GetProductContentsByIdAsync(
            IReadOnlyList<int> keys,
            ISender sender,
            CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new GetProductsContentsQuery(keys),
                cancellationToken))
            .Contents
            .ToDictionary(
                x => x.Key,
                x => x.Value
                    .Select(z => new GqlProductContent(z))
                    .ToList());
    }

    [DataLoader]
    public static async Task<Dictionary<GetProductCrossesItem, IReadOnlyList<GqlProduct>>>
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
                IReadOnlyList<GqlProduct> (x) => x.Value
                    .Select(product => new GqlProduct(product))
                    .ToArray());
    }
}
