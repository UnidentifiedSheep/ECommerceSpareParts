using GraphQL.Common.Extensions;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.DataLoaders.Product;
using Main.Api.GraphQl.Queries.Root;

namespace Main.Api.GraphQl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQlServices(
        this IServiceCollection services,
        string name)
    {
        services.AddCommonGraphQl(name)
            .AddQueryType<Query>()
            .AddDataLoader<CatalogueCandidateByIdDataLoader>()
            .AddDataLoader<ProductSizeByIdDataLoader>()
            .AddDataLoader<ProductWeightByIdDataLoader>()
            .AddDataLoader<ProductByIdDataLoader>()
            .AddDataLoader<ProducerByIdDataLoader>();
        
        return services;
    }
}
