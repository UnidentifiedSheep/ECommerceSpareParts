using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Application.Models.CoPurchase;

public sealed record CoPurchaseItemSegments(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("co.purchase.segments.has.related.name")]
	[property: SchemaFieldDescription("co.purchase.segments.has.related.description")]
	bool HasRelated);
