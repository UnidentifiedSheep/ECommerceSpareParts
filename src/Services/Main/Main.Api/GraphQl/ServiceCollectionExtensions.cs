using GraphQL.Common.Extensions;
using Main.Api.GraphQl.Mutations;
using Main.Api.GraphQl.Queries;

namespace Main.Api.GraphQl;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGraphQlServices(this IServiceCollection services, string name)
	{
		services.AddMainDataLoaders();

		services.AddCommonGraphQl(name)
			.AddMutationType<RootMutation>()
			.AddQueryType<RootQuery>();

		return services;
	}
}
