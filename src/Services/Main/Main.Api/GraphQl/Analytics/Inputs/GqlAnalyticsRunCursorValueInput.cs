using HotChocolate;

namespace Main.Api.GraphQl.Analytics.Inputs;

[GraphQLName("AnalyticsRunCursorValueInput")]
public readonly record struct GqlAnalyticsRunCursorValueInput
{
	[GraphQLName("id")]
	public required Guid Id { get; init; }

	[GraphQLName("createdAt")]
	public required DateTime CreatedAt { get; init; }
}
