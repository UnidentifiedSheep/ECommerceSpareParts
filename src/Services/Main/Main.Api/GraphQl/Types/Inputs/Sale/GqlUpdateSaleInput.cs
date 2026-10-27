using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Sale;

[GraphQLName("UpdateSaleInput")]
public sealed record GqlUpdateSaleInput
{
	[GraphQLName("saleId")]
	public required Guid SaleId { get; init; }

	[GraphQLName("rowVersion")]
	public required uint RowVersion { get; init; }

	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("saleDateTime")]
	public required DateTime SaleDateTime { get; init; }

	[GraphQLName("contents")]
	public required IReadOnlyList<GqlEditSaleContentInput> Contents { get; init; }

	[GraphQLName("comment")]
	public string? Comment { get; init; }

	[GraphQLName("confirmationCode")]
	public string? ConfirmationCode { get; init; }

	[GraphQLName("forcePayment")]
	public bool? ForcePayment { get; init; }
}
