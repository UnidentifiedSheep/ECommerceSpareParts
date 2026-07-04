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

        await InitializeIfMissing(
            idx,
            ct => Client.Indices.CreateAsync(
                idx,
                c => c
                    .Settings(s => s
                        .Analysis(a => a
                            .Normalizers(n => n
                                .Custom(
                                    "lowercase_normalizer",
                                    cn => cn
                                        .Filters("lowercase")
                                )
                            )
                        ))
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
                            .Keyword(k => k
                                .Name(x => x.NormalizedSku)
                                .Normalizer("lowercase_normalizer")
                            )
                            .Number(n => n
                                .Name(x => x.ProducerId)
                                .Type(NumberType.Integer)
                            )
                            .Text(t => t
                                .Name(x => x.Indicator)
                                .Index(false)
                            )
                            .Number(n => n
                                .Name(x => x.Stock)
                                .Type(NumberType.Integer))
                            .Object<ProductDimensions>(o => o
                                .Name(x => x.Dimensions)
                                .Properties(dp => dp
                                    .Number(n => n
                                        .Name(d => d.Length)
                                        .Type(NumberType.Double)
                                    )
                                    .Number(n => n
                                        .Name(d => d.LengthM)
                                        .Type(NumberType.Double)
                                    )
                                    .Number(n => n
                                        .Name(d => d.Width)
                                        .Type(NumberType.Double)
                                    )
                                    .Number(n => n
                                        .Name(d => d.WidthM)
                                        .Type(NumberType.Double)
                                    )
                                    .Number(n => n
                                        .Name(d => d.Height)
                                        .Type(NumberType.Double)
                                    )
                                    .Number(n => n
                                        .Name(d => d.HeightM)
                                        .Type(NumberType.Double)
                                    )
                                    .Number(n => n
                                        .Name(d => d.Unit)
                                        .Type(NumberType.Integer)
                                    )
                                    .Number(n => n
                                        .Name(d => d.VolumeM3)
                                        .Type(NumberType.Double)
                                    )
                                )
                            )
                            .Object<ProductWeight>(o => o
                                .Name(x => x.Weight)
                                .Properties(wp => wp
                                    .Number(n => n
                                        .Name(w => w.Value)
                                        .Type(NumberType.Double)
                                    )
                                    .Number(n => n
                                        .Name(w => w.Unit)
                                        .Type(NumberType.Integer)
                                    )
                                    .Number(n => n
                                        .Name(w => w.WeightKg)
                                        .Type(NumberType.Double)
                                    )
                                )
                            )
                        )
                    ),
                ct),
            cancellationToken);
    }
}