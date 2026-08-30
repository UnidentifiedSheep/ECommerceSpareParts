using OpenSearch.Client;

namespace Search.Persistence.Extensions;

internal static class CatalogueSearchHighlightExtensions
{
	public const string MarkerStart = "[[[";

	public const string MarkerEnd = "]]]";

	internal static SearchDescriptor<TDocument> AddCatalogueHighlights<TDocument>(
		this SearchDescriptor<TDocument> search,
		bool includeHighlights,
		string query,
		Field skuField,
		Field normalizedSkuField,
		Field nameField) where TDocument : class
	{
		if (!includeHighlights || string.IsNullOrWhiteSpace(query))
			return search;

		return search.Highlight(highlight => highlight
			.PreTags(MarkerStart)
			.PostTags(MarkerEnd)
			.Fields(
				field => field
					.Field(skuField)
					.Type(HighlighterType.Unified)
					.NumberOfFragments(0)
					.MatchedFields(fields => fields
						.Field(skuField)
						.Field(normalizedSkuField)
						.Field($"{normalizedSkuField.Name}.prefix")
						.Field($"{normalizedSkuField.Name}.contains")),
				field => field
					.Field(nameField)
					.Type(HighlighterType.Unified)
					.NumberOfFragments(5)
					.MatchedFields(fields => fields
						.Field(nameField)
						.Field($"{nameField.Name}.keyword")
						.Field($"{nameField.Name}.prefix")
						.Field($"{nameField.Name}.contains"))));
	}
}
