using HotChocolate;

namespace Search.Api.GraphQl.Types.Highlights;

[GraphQLName("Highlights")]
public record GqlHighlights(
    [property: GraphQLName("items")]
    IReadOnlyCollection<GqlHighlight> Items)
{
    public static GqlHighlights? From(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>>? highlights)
        => highlights == null ? null : new GqlHighlights(highlights
            .Select(x => new GqlHighlight(x.Key, x.Value))
            .ToArray());
}