using GraphQL.Common.Authorization;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IRequestExecutorBuilder AddCommonGraphQl(
        this IServiceCollection services)
    {
        return services
            .AddGraphQLServer()
            .AddCommonAuthorization();
    }
}
