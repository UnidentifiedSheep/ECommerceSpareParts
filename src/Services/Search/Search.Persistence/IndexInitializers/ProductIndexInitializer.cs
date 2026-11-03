using Search.Enums;
﻿using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Entities;
using Search.Persistence.Abstractions;

namespace Search.Persistence.IndexInitializers;

public class ProductIndexInitializer(IOpenSearchClient client, IOptions<OpenSearchOptions> options)
	: IndexInitializerBase<Product>(client, TimeSpan.FromHours(3))
{
	private const int TotalFieldsLimit = 10000;

	public override async Task LazyInitialize(CancellationToken cancellationToken = default)
	{
		var idx = options.Value.IndexOptions.Products;

		await InitializeIfMissing(
			idx,
			ct => Client.Indices.CreateAsync(
				idx,
				c => c
					.Settings(s => s
						.Setting("index.max_ngram_diff", 18)
						.Setting("index.mapping.total_fields.limit", TotalFieldsLimit)
						.Analysis(a => a.ConfigureCatalogueSearch()))
					.Map<Product>(m => m.Properties(p =>
						p.StockFields()
							.Number(number => number.Name(x => x.ReadinessStatus).Type(NumberType.Integer))
							.Date(d => d.Name(x => x.ProductUpdatedAt))
							.Date(d => d.Name(x => x.CharacteristicsUpdatedAt).Index(false).DocValues(false))
							.Date(d => d.Name(x => x.DimensionsUpdatedAt).Index(false).DocValues(false))
							.Date(d => d.Name(x => x.WeightUpdatedAt).Index(false).DocValues(false))
							.Nested<ProductCharacteristic>(n => n.Name(x => x.Characteristics).Properties(cp => cp
								.Number(k => k.Name(x => x.CharacteristicId).Type(NumberType.Integer))
								.Keyword(k => k.Name(x => x.Value))
								.Number(k => k.Name(x => x.NumericValue).Type(NumberType.Double))))
							.Keyword(k => k.Name(x => x.Id))
							.Text(text => text
								.Name(product => product.Name)
								.Analyzer(CatalogueSearchAnalysis.SearchAnalyzer)
								.Fields(fields => fields.Keyword(keyword => keyword
									.Name("keyword")
									.IgnoreAbove(256)
									.Normalizer(CatalogueSearchAnalysis.LowercaseNormalizer))))
							.Keyword(k => k.Name(x => x.Sku).Normalizer("lowercase_normalizer"))
							.Keyword(k => k
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
							.Number(n => n.Name(x => x.ProducerId).Type(NumberType.Integer))
							.Number(n => n.Name(x => x.ProductGroupId).Type(NumberType.Integer))
							.Text(t => t.Name(x => x.Indicator).Index(false))
							.Object<ProductDimensions>(o => o
								.Name(x => x.Dimensions)
								.Properties(dp => dp
									.Number(n => n.Name(d => d.Length).Type(NumberType.Double))
									.Number(n => n.Name(d => d.LengthM).Type(NumberType.Double))
									.Number(n => n.Name(d => d.Width).Type(NumberType.Double))
									.Number(n => n.Name(d => d.WidthM).Type(NumberType.Double))
									.Number(n => n.Name(d => d.Height).Type(NumberType.Double))
									.Number(n => n.Name(d => d.HeightM).Type(NumberType.Double))
									.Number(n => n.Name(d => d.Unit).Type(NumberType.Integer))
									.Number(n => n.Name(d => d.VolumeM3).Type(NumberType.Double))))
							.Object<ProductWeight>(o => o
								.Name(x => x.Weight)
								.Properties(wp => wp
									.Number(n => n.Name(w => w.Value).Type(NumberType.Double))
									.Number(n => n.Name(w => w.Unit).Type(NumberType.Integer))
									.Number(n => n.Name(w => w.WeightKg).Type(NumberType.Double)))))),
				ct),
			cancellationToken);
	}

}
