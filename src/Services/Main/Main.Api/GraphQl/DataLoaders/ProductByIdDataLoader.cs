using GreenDonut;
using Main.Api.GraphQl.Types;
using Main.Application.Handlers.Products;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public class ProductByIdDataLoader(
    ISender sender,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options)
    : BatchDataLoader<int, GqlProduct>(batchScheduler, options)
{
    protected override async Task<IReadOnlyDictionary<int, GqlProduct>>
        LoadBatchAsync(
            IReadOnlyList<int> keys,
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
}