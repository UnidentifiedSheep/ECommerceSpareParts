using Analytics.Api.GraphQl.Queries.Root;
using GraphQL.Common.Extensions;
using HotChocolate.Types;

namespace Analytics.Api.GraphQl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQlServices(
        this IServiceCollection services,
        string name)
    {
        services.AddCommonGraphQl(name)
            .AddQueryType<Query>(descriptor =>
                descriptor
                    .Field("_empty")
                    .Type<NonNullType<BooleanType>>()
                    .Resolve(_ => new ValueTask<object?>(true)));

        return services;
    }
}
