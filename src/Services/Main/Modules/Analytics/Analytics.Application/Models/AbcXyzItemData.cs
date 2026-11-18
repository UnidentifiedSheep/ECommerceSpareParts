using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Application.Models;

public sealed record AbcXyzItemData(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.data.quantity.name")]
	[property: SchemaFieldDescription("abc.xyz.data.quantity.description")]
	decimal Quantity,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.data.revenue.name")]
	[property: SchemaFieldDescription("abc.xyz.data.revenue.description")]
	decimal Revenue,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.data.cumulative.share.name")]
	[property: SchemaFieldDescription("abc.xyz.data.cumulative.share.description")]
	decimal CumulativeShare,
	[property: SchemaFieldLabel("abc.xyz.data.coefficient.of.variation.name")]
	[property: SchemaFieldDescription("abc.xyz.data.coefficient.of.variation.description")]
	decimal? CoefficientOfVariation,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.data.periods.count.name")]
	[property: SchemaFieldDescription("abc.xyz.data.periods.count.description")]
	int PeriodsCount,
	[property: SchemaFieldLabel("abc.xyz.data.adi.name")]
	[property: SchemaFieldDescription("abc.xyz.data.adi.description")]
	decimal? Adi = null,
	[property: SchemaFieldLabel("abc.xyz.data.cv2.name")]
	[property: SchemaFieldDescription("abc.xyz.data.cv2.description")]
	decimal? Cv2 = null);
