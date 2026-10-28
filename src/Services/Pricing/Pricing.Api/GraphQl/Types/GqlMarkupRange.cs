using HotChocolate;
using Pricing.Application.Dtos.Markup;

namespace Pricing.Api.GraphQl.Types;

[GraphQLName("MarkupRange")]
public sealed record GqlMarkupRange([property: GraphQLIgnore] MarkupRangeDto Range)
{
	[GraphQLName("id")]
	public int Id => Range.Id;

	[GraphQLName("rangeStart")]
	public decimal RangeStart => Range.RangeStart;

	[GraphQLName("rangeEnd")]
	public decimal RangeEnd => Range.RangeEnd;

	[GraphQLName("markup")]
	public decimal Markup => Range.Markup;
}
