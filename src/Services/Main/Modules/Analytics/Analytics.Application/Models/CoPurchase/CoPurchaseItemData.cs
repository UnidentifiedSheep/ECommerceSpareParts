using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Application.Models.CoPurchase;

public sealed record CoPurchaseItemData(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.data.sales.count.name")]
	[property: SchemaFieldDescription("co.purchase.data.sales.count.description")]
	int SalesCount,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.data.related.name")]
	[property: SchemaFieldDescription("co.purchase.data.related.description")]
	IReadOnlyList<CoPurchaseRelated> Related);
