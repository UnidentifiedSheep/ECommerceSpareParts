using HotChocolate;

namespace Search.Api.GraphQl.Types.Highlights;

[GraphQLName("Highlight")]
public record GqlHighlight(
	[property: GraphQLName("field")]
	string Field,
	[property: GraphQLName("fragments")]
	IReadOnlyCollection<string> Fragments);
