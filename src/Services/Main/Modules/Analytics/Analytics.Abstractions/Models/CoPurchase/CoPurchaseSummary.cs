using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Abstractions.Models.CoPurchase;

public sealed record CoPurchaseSummary(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.summary.products.count.name")]
	[property: SchemaFieldDescription("co.purchase.summary.products.count.description")]
	int ProductsCount,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.summary.products.with.related.count.name")]
	[property: SchemaFieldDescription("co.purchase.summary.products.with.related.count.description")]
	int ProductsWithRelatedCount,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.summary.sales.count.name")]
	[property: SchemaFieldDescription("co.purchase.summary.sales.count.description")]
	int SalesCount);
