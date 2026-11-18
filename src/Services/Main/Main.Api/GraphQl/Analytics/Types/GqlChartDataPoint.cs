using System.Text.Json;
using HotChocolate;
using HotChocolate.Types;
using Main.Modules.Analytics.Abstractions.Models;

namespace Main.Api.GraphQl.Analytics.Types;

[GraphQLName("ChartDataPoint")]
public sealed record GqlChartDataPoint(
	[property: GraphQLIgnore]
	JsonElement Data,
	[property: GraphQLIgnore]
	IReadOnlyList<ChartReference> Refs)
{
	[GraphQLName("value")]
	[GraphQLType<NonNullType<AnyType>>]
	public JsonElement Value => Data;

	[GraphQLName("references")]
	public IReadOnlyList<GqlChartDataPointReference> References
		=> Refs
			.Where(reference => reference.Key is not null)
			.Select(reference => new GqlChartDataPointReference(
				reference.Field,
				reference.Entity,
				ChartReferenceFactory.Create(reference)))
			.ToArray();
}
