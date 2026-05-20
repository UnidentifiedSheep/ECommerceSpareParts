using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Entities;
using Search.Persistence.Abstractions;

namespace Search.Persistence.IndexInitializers;

public class ProductIndexInitializer(
    IOpenSearchClient client,
    IOptions<OpenSearchOptions> options
    ) : IndexInitializerBase<Product>(client, TimeSpan.FromHours(3))
{
    public override async Task LazyInitialize(
        CancellationToken cancellationToken = default)
    {
        var idx = options.Value.IndexOptions.Products;

        if (await CheckIfIndexExists(idx, cancellationToken))
            return;
        
        await Client.Indices.CreateAsync(idx, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Normalizers(n => n
                        .Custom("lowercase_normalizer", cn => cn
                            .Filters("lowercase")
                        )
                    )
                )
            )
            .Map<Product>(m => m
                .Properties(p => p
                    .Keyword(k => k
                        .Name(x => x.Id)
                    )
                    .Text(t => t
                        .Name(x => x.Name)
                        .Fields(f => f
                            .Keyword(k => k
                                .Name("keyword")
                                .IgnoreAbove(256)
                            )
                        )
                    )
                    .Keyword(k => k
                        .Name(x => x.Sku)
                        .Normalizer("lowercase_normalizer")
                    )
                )
            ), cancellationToken);
    }
}