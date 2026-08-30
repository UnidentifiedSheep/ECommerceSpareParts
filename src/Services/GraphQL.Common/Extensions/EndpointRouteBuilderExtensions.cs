using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace GraphQL.Common.Extensions;

public static class EndpointRouteBuilderExtensions
{
	public static IEndpointConventionBuilder MapCommonGraphQl(
		this IEndpointRouteBuilder endpoints,
		string path = "/graphql") => endpoints.MapGraphQL(path);
}
