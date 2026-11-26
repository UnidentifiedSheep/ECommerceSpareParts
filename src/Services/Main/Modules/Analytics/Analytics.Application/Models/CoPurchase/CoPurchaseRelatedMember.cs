using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Application.Models.CoPurchase;

public sealed record CoPurchaseRelatedMember(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.related.member.product.id.name")]
	[property: SchemaFieldDescription("co.purchase.related.member.product.id.description")]
	int ProductId,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.related.member.pair.count.name")]
	[property: SchemaFieldDescription("co.purchase.related.member.pair.count.description")]
	int PairCount);
