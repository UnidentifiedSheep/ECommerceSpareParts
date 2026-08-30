using GraphQL.Common.Types;
using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Common.Extensions;

public static class ServiceCollectionExtensions
{
	public static IRequestExecutorBuilder AddCommonGraphQl(this IServiceCollection services, string name)
	{
		return services
			.AddGraphQLServer(name)
			.ModifyServerOptions(options =>
			{
				options.Batching = AllowedBatching.All;
			})
			.AddCommonAuthorization()
			.AddType<GqlPagination>()
			.AddType<GqlSortBy>();
	}
}
