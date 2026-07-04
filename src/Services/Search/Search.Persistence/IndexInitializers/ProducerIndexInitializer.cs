using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Entities;
using Search.Persistence.Abstractions;

namespace Search.Persistence.IndexInitializers;

public class ProducerIndexInitializer(
    IOpenSearchClient client,
    IOptions<OpenSearchOptions> options
) : IndexInitializerBase<Producer>(client, TimeSpan.FromHours(3))
{
    public override async Task LazyInitialize(
        CancellationToken cancellationToken = default)
    {
        var idx = options.Value.IndexOptions.Producers;

        await InitializeIfMissing(
            idx,
            ct => Client.Indices.CreateAsync(
                idx,
                c => c
                    .Settings(s => s
                        .Setting("index.max_ngram_diff", 18)
                        .Analysis(a => a
                            .Tokenizers(t => t
                                .NGram(
                                    "producer_ngram_tokenizer",
                                    nt => nt
                                        .MinGram(2)
                                        .MaxGram(20)
                                        .TokenChars(
                                            TokenChar.Letter,
                                            TokenChar.Digit)
                                )
                            )
                            .Analyzers(an => an
                                .Custom(
                                    "producer_ngram_analyzer",
                                    ca => ca
                                        .Tokenizer("producer_ngram_tokenizer")
                                        .Filters("lowercase")
                                )
                            )
                            .Normalizers(n => n
                                .Custom(
                                    "lowercase_normalizer",
                                    cn => cn
                                        .Filters("lowercase")
                                )
                            )
                        ))
                    .Map<Producer>(m => m
                        .Dynamic(false)
                        .Properties(p => p
                            .Keyword(k => k
                                .Name(x => x.Id)
                            )
                            .Text(t => t
                                .Name(x => x.Name)
                                .Analyzer("producer_ngram_analyzer")
                                .SearchAnalyzer("standard")
                                .Fields(f => f
                                    .Keyword(k => k
                                        .Name("keyword")
                                        .IgnoreAbove(256)
                                        .Normalizer("lowercase_normalizer")
                                    )
                                )
                            )
                            .Text(t => t
                                .Name(x => x.Description)
                                .Index(false)
                            )
                            .Object<ProducerAlias>(o => o
                                .Name(x => x.Aliases)
                                .Dynamic(false)
                                .Properties(op => op
                                    .Text(t => t
                                        .Name(x => x.Alias)
                                        .Analyzer("producer_ngram_analyzer")
                                        .SearchAnalyzer("standard")
                                        .Fields(f => f
                                            .Keyword(k => k
                                                .Name("keyword")
                                                .IgnoreAbove(256)
                                                .Normalizer("lowercase_normalizer")
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ),
                ct),
            cancellationToken);
    }
}