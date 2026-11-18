using HotChocolate;
using HotChocolate.Types;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Api.GraphQl.Analytics.Types;

[GraphQLName("ChartDataPointReference")]
public sealed record GqlChartDataPointReference(
	[property: GraphQLIgnore]
	string FieldName,
	[property: GraphQLIgnore]
	SchemaEntity EntityType,
	[property: GraphQLIgnore]
	object TargetValue)
{
	[GraphQLName("field")]
	public string Field => FieldName;

	[GraphQLName("entity")]
	public string Entity => EntityType.ToString();

	[GraphQLName("target")]
	[GraphQLType<NonNullType<ChartReferenceTargetType>>]
	public object Target => TargetValue;
}
