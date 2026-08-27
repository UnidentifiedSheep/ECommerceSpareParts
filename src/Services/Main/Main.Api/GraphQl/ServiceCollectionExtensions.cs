using GraphQL.Common.Extensions;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Queries.Root;

namespace Main.Api.GraphQl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQlServices(
        this IServiceCollection services)
    {
        services.AddCommonGraphQl()
            .AddQueryType<RootQuery>()
            .AddDataLoader<ProducerByIdDataLoader>();
        
        return services;
    }
}
