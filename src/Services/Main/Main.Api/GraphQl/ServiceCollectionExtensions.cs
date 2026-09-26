using GraphQL.Common.Extensions;
using Main.Api.GraphQl.Mutations;
using Main.Api.GraphQl.Queries;
using Main.Api.GraphQl.Subscriptions;

namespace Main.Api.GraphQl;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGraphQlServices(this IServiceCollection services, string name)
	{
		services.AddCommonGraphQl(name)
			.AddMutationType<RootMutation>()
			.AddQueryType<RootQuery>()
			.AddMainGraphQL();

		return services;
	}
}
