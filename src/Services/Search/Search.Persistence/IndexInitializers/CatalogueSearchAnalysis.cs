using OpenSearch.Client;

namespace Search.Persistence.IndexInitializers;

internal static class CatalogueSearchAnalysis
{
	internal const string LowercaseNormalizer = "lowercase_normalizer";

	internal const string SearchAnalyzer = "catalogue_search";

	internal const string PrefixAnalyzer = "catalogue_prefix";

	internal const string ContainsAnalyzer = "catalogue_contains";

	internal static AnalysisDescriptor ConfigureCatalogueSearch(this AnalysisDescriptor analysis)
	{
		return analysis
			.Normalizers(normalizers => normalizers.Custom(
				LowercaseNormalizer,
				normalizer => normalizer.Filters("lowercase")))
			.Tokenizers(tokenizers => tokenizers
				.EdgeNGram(
					"catalogue_prefix_tokenizer",
					tokenizer =>
						tokenizer.MinGram(2).MaxGram(20).TokenChars(TokenChar.Letter, TokenChar.Digit))
				.NGram(
					"catalogue_contains_tokenizer",
					tokenizer =>
						tokenizer.MinGram(2).MaxGram(20).TokenChars(TokenChar.Letter, TokenChar.Digit)))
			.Analyzers(analyzers => analyzers
				.Custom(SearchAnalyzer, analyzer => analyzer.Tokenizer("standard").Filters("lowercase"))
				.Custom(
					PrefixAnalyzer,
					analyzer => analyzer.Tokenizer("catalogue_prefix_tokenizer").Filters("lowercase"))
				.Custom(
					ContainsAnalyzer,
					analyzer => analyzer.Tokenizer("catalogue_contains_tokenizer").Filters("lowercase")));
	}
}
