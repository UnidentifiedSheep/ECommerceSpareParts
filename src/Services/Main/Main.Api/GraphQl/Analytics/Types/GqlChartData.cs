using System.Text.Json;
using HotChocolate;
using HotChocolate.Types;

namespace Main.Api.GraphQl.Analytics.Types;

[GraphQLName("ChartData")]
public sealed record GqlChartData
{
	[GraphQLName("dataPoints")]
	[GraphQLType<NonNullType<ListType<NonNullType<AnyType>>>>]
	public required IReadOnlyList<JsonElement> DataPoints { get; init; }

	[GraphQLName("nextCursor")]
	public string? NextCursor { get; init; }
}
