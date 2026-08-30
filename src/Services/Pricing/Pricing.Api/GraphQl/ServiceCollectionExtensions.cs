using GraphQL.Common.Extensions;
using HotChocolate.Types;
using Pricing.Api.GraphQl.Queries.Root;

namespace Pricing.Api.GraphQl;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGraphQlServices(this IServiceCollection services, string name)
	{
		services
			.AddCommonGraphQl(name)
			.AddQueryType<Query>(descriptor =>
				descriptor
					.Field("_pricing")
					.Type<NonNullType<BooleanType>>()
					.Resolve(_ => new ValueTask<object?>(true)));

		return services;
	}
}
