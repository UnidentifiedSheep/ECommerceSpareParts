using GraphQL.Common.Extensions;
using Search.Api.GraphQl.Queries.Root;

namespace Search.Api.GraphQl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQlServices(
        this IServiceCollection services,
        string name)
    {
        services.AddCommonGraphQl(name)
            .AddQueryType<Query>();
        
        return services;
    }
}
