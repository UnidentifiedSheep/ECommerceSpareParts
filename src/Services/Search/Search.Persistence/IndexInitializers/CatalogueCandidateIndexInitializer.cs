using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Entities;
using Search.Persistence.Abstractions;

namespace Search.Persistence.IndexInitializers;

public sealed class CatalogueCandidateIndexInitializer(
    IOpenSearchClient client,
    IOptions<OpenSearchOptions> options)
    : IndexInitializerBase<CatalogueCandidate>(client, TimeSpan.FromHours(3))
{
    public override async Task LazyInitialize(
        CancellationToken cancellationToken = default)
    {
        var index = options.Value.IndexOptions.CatalogueCandidates;

        await InitializeIfMissing(
            index,
            ct => Client.Indices.CreateAsync(
                index,
                descriptor => descriptor
                    .Settings(settings => settings
                        .Setting("index.max_ngram_diff", 18)
                        .Analysis(analysis => analysis
                            .ConfigureCatalogueSearch()))
                    .Map<CatalogueCandidate>(mapping => mapping
                        .Dynamic(false)
                        .Properties(properties => properties
                            .Keyword(keyword => keyword
                                .Name(x => x.Id))
                            .Keyword(keyword => keyword
                                .Name(x => x.Sku)
                                .Normalizer("lowercase_normalizer"))
                            .Keyword(keyword => keyword
                                .Name(x => x.NormalizedSku)
                                .Normalizer(CatalogueSearchAnalysis.LowercaseNormalizer)
                                .Fields(fields => fields
                                    .Text(prefix => prefix
                                        .Name("prefix")
                                        .Analyzer(CatalogueSearchAnalysis.PrefixAnalyzer)
                                        .SearchAnalyzer(CatalogueSearchAnalysis.SearchAnalyzer))
                                    .Text(contains => contains
                                        .Name("contains")
                                        .Analyzer(CatalogueSearchAnalysis.ContainsAnalyzer)
                                        .SearchAnalyzer(CatalogueSearchAnalysis.SearchAnalyzer))))
                            .Number(number => number
                                .Name(x => x.ProducerId)
                                .Type(NumberType.Integer))
                            .Number(number => number
                                .Name(x => x.MappedProductId)
                                .Type(NumberType.Integer))
                            .Text(text => text
                                .Name(x => x.Names)
                                .Analyzer(CatalogueSearchAnalysis.SearchAnalyzer)
                                .Fields(fields => fields
                                    .Keyword(keyword => keyword
                                        .Name("keyword")
                                        .IgnoreAbove(256)
                                        .Normalizer(CatalogueSearchAnalysis.LowercaseNormalizer))
                                    .Text(prefix => prefix
                                        .Name("prefix")
                                        .Analyzer(CatalogueSearchAnalysis.PrefixAnalyzer)
                                        .SearchAnalyzer(CatalogueSearchAnalysis.SearchAnalyzer))
                                    .Text(contains => contains
                                        .Name("contains")
                                        .Analyzer(CatalogueSearchAnalysis.ContainsAnalyzer)
                                        .SearchAnalyzer(CatalogueSearchAnalysis.SearchAnalyzer)))))),
                ct),
            cancellationToken);
    }
}
