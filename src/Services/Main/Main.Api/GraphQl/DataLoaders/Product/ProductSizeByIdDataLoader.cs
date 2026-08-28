using GreenDonut;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Handlers.ProductSizes;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders.Product;

public class ProductSizeByIdDataLoader(
    ISender sender,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options)
    : BatchDataLoader<int, GqlProductSize>(batchScheduler, options)
{
    protected override async Task<IReadOnlyDictionary<int, GqlProductSize>>
        LoadBatchAsync(
            IReadOnlyList<int> keys,
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
}