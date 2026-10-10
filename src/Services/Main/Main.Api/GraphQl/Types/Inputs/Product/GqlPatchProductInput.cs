using GraphQL.Common.Types;
using HotChocolate;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("PatchProductInput")]
public sealed record GqlPatchProductInput
{
	[GraphQLName("sku")]
	public GqlPatchField<string>? Sku { get; init; }

	[GraphQLName("name")]
	public GqlPatchField<string>? Name { get; init; }

	[GraphQLName("producerId")]
	public GqlPatchField<int>? ProducerId { get; init; }

	[GraphQLName("description")]
	public GqlPatchField<string?>? Description { get; init; }

	[GraphQLName("packingUnit")]
	public GqlPatchField<int?>? PackingUnit { get; init; }

	[GraphQLName("indicator")]
	public GqlPatchField<string?>? Indicator { get; init; }

	[GraphQLName("groupId")]
	public GqlPatchField<int?>? GroupId { get; init; }

	[GraphQLName("pairId")]
	public GqlPatchField<int?>? PairId { get; init; }

	public PatchProductDto ToDto() => new()
	{
		Sku = Sku,
		Name = Name,
		ProducerId = ProducerId,
		Description = Description,
		PackingUnit = PackingUnit,
		Indicator = Indicator,
		GroupId = GroupId,
		PairId = PairId
	};
}
