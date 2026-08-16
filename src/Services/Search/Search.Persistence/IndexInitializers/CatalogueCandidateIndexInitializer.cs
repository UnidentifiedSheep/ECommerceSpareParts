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
                        .Analysis(analysis => analysis
                            .Normalizers(normalizers => normalizers
                                .Custom(
                                    "lowercase_normalizer",
                                    normalizer => normalizer
                                        .Filters("lowercase")))))
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
                                .Normalizer("lowercase_normalizer"))
                            .Number(number => number
                                .Name(x => x.ProducerId)
                                .Type(NumberType.Integer))
                            .Number(number => number
                                .Name(x => x.MappedProductId)
                                .Type(NumberType.Integer))
                            .Text(text => text
                                .Name(x => x.Names)
                                .Fields(fields => fields
                                    .Keyword(keyword => keyword
                                        .Name("keyword")
                                        .IgnoreAbove(256)
                                        .Normalizer("lowercase_normalizer")))))),
                ct),
            cancellationToken);
    }
}
