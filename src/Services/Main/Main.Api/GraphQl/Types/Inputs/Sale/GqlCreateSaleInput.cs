using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Sale;

[GraphQLName("CreateSaleInput")]
public sealed record GqlCreateSaleInput
{
	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("organizationId")]
	public required Guid OrganizationId { get; init; }

	[GraphQLName("storageCode")]
	public required string StorageCode { get; init; }

	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("saleDateTime")]
	public required DateTime SaleDateTime { get; init; }

	[GraphQLName("contents")]
	public required IReadOnlyList<GqlNewSaleContentInput> Contents { get; init; }

	[GraphQLName("comment")]
	public string? Comment { get; init; }

	[GraphQLName("payedSum")]
	public decimal? PayedSum { get; init; }

	[GraphQLName("confirmationCode")]
	public string? ConfirmationCode { get; init; }

	[GraphQLName("forcePayment")]
	public bool? ForcePayment { get; init; }
}
