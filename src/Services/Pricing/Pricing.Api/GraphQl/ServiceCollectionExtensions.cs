using GraphQL.Common.Extensions;
using HotChocolate.Types;
using Pricing.Api.GraphQl.Mutations;
using Pricing.Api.GraphQl.Queries.Root;

namespace Pricing.Api.GraphQl;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGraphQlServices(this IServiceCollection services, string name)
	{
		services
			.AddCommonGraphQl(name)
			.AddQueryType<Query>()
			.AddMutationType<RootMutation>()
			.AddPricingGraphQL();

		return services;
	}
}
