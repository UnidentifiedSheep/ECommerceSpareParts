using HotChocolate;
using Search.Application.Models.CatalogueSearch.Criteria;

namespace Search.Api.GraphQl.Types.Inputs.CatalogueSearch.Criteria;

[GraphQLName("ProductCharacteristicCriteriaInput")]
public sealed record GqlCharacteristicSearchCriteriaInput
{
	[GraphQLName("characteristicId")]
	public required int CharacteristicId { get; init; }

	[GraphQLName("values")]
	public IReadOnlyList<string>? Values { get; init; }

	[GraphQLName("numericRange")]
	public GqlNumericRangeInput? NumericRange { get; init; }

	[GraphQLName("exists")]
	public bool? Exists { get; init; }

	public CharacteristicSearchCriteria ToCriteria() => new()
	{
		CharacteristicId = CharacteristicId, Values = Values, NumericRange = NumericRange, Exists = Exists
	};
}
