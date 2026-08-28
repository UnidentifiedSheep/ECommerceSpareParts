using GreenDonut;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Handlers.ProductWeight;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders.Product;

public class ProductWeightByIdDataLoader(
    ISender sender,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options)
    : BatchDataLoader<int, GqlProductWeight>(batchScheduler, options)
{
    protected override async Task<IReadOnlyDictionary<int, GqlProductWeight>>
        LoadBatchAsync(
            IReadOnlyList<int> keys,
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
}