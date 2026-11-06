using HotChocolate;
using Main.Application.Orders.Dtos;

namespace Main.Api.GraphQl.Types.Inputs.Order;

[GraphQLName("UpsertOrderInput")]
public sealed record GqlUpsertOrderInput
{
	[GraphQLName("id")]
	public Guid? Id { get; init; }
	[GraphQLName("rowVersion")]
	public uint? RowVersion { get; init; }
	[GraphQLName("organizationId")]
	public required Guid OrganizationId { get; init; }
	[GraphQLName("userId")]
	public Guid? UserId { get; init; }
	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }
	[GraphQLName("items")]
	public required IReadOnlyList<GqlUpsertOrderItemInput> Items { get; init; }

	public UpsertOrderDto ToDto() => new()
	{
		Id = Id, RowVersion = RowVersion, OrganizationId = OrganizationId, UserId = UserId,
		CurrencyId = CurrencyId, Items = Items.Select(item => item.ToDto()).ToArray()
	};
}

[GraphQLName("UpsertOrderItemInput")]
public sealed record GqlUpsertOrderItemInput
{
	[GraphQLName("id")]
	public int? Id { get; init; }
	[GraphQLName("productId")]
	public required int ProductId { get; init; }
	[GraphQLName("count")]
	public required int Count { get; init; }
	[GraphQLName("unitPrice")]
	public decimal? UnitPrice { get; init; }

	public UpsertOrderItemDto ToDto() => new() { Id = Id, ProductId = ProductId, Count = Count, UnitPrice = UnitPrice };
}
