using HotChocolate;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Types.Inputs.Product;

[GraphQLName("CreateProductInput")]
public sealed record GqlCreateProductInput
{
	[GraphQLName("sku")]
	public required string Sku { get; init; }

	[GraphQLName("name")]
	public required string Name { get; init; }

	[GraphQLName("producerId")]
	public required int ProducerId { get; init; }

	[GraphQLName("description")]
	public string? Description { get; init; }

	[GraphQLName("indicator")]
	public string? Indicator { get; init; }

	[GraphQLName("groupId")]
	public int? GroupId { get; init; }

	public CreateProductDto ToDto() => new()
	{
		Sku = Sku,
		Name = Name,
		ProducerId = ProducerId,
		Description = Description,
		Indicator = Indicator,
		GroupId = GroupId
	};
}
