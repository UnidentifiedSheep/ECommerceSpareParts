using GraphQL.Common.Types;
using HotChocolate;
using Main.Application.Dtos.Storage;

namespace Main.Api.GraphQl.Types.Inputs.Storage;

[GraphQLName("EditStorageContentInput")]
public sealed record GqlEditStorageContentInput
{
	[GraphQLName("id")]
	public required int Id { get; init; }

	[GraphQLName("rowVersion")]
	public required uint RowVersion { get; init; }

	[GraphQLName("count")]
	public GqlPatchField<int>? Count { get; init; }

	[GraphQLName("purchaseDatetime")]
	public GqlPatchField<DateTime>? PurchaseDatetime { get; init; }

	[GraphQLName("buyPrice")]
	public GqlPatchField<decimal>? BuyPrice { get; init; }

	[GraphQLName("currencyId")]
	public GqlPatchField<int>? CurrencyId { get; init; }

	public PatchStorageContentDto ToDto()
		=> new()
		{
			Count = Count,
			PurchaseDatetime = PurchaseDatetime,
			BuyPrice = BuyPrice,
			CurrencyId = CurrencyId
		};
}
