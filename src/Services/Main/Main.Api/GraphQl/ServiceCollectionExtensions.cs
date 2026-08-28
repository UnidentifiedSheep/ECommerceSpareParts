using GraphQL.Common.Extensions;
using Main.Api.GraphQl.Queries.Root;

namespace Main.Api.GraphQl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQlServices(
        this IServiceCollection services,
        string name)
    {
        services.AddMainDataLoaders();

        services.AddCommonGraphQl(name)
            .AddQueryType<Query>();
        
        return services;
    }
}
