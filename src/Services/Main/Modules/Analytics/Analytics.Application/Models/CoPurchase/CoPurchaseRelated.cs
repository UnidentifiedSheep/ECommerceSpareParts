using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Application.Models.CoPurchase;

public sealed record CoPurchaseRelated(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.related.members.name")]
	[property: SchemaFieldDescription("co.purchase.related.members.description")]
	IReadOnlyList<CoPurchaseRelatedMember> Members,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.related.pair.count.name")]
	[property: SchemaFieldDescription("co.purchase.related.pair.count.description")]
	int PairCount,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.related.confidence.name")]
	[property: SchemaFieldDescription("co.purchase.related.confidence.description")]
	decimal Confidence,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.related.lift.name")]
	[property: SchemaFieldDescription("co.purchase.related.lift.description")]
	decimal Lift);
