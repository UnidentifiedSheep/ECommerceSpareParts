using HotChocolate;
using Main.Enums.Balances;

namespace Main.Api.GraphQl.Types.Inputs.Transaction;

[GraphQLName("CreateTransactionInput")]
public sealed record GqlCreateTransactionInput
{
	[GraphQLName("organizationId")]
	public required Guid OrganizationId { get; init; }

	[GraphQLName("direction")]
	public required SystemTransactionDirection Direction { get; init; }

	[GraphQLName("amount")]
	public required decimal Amount { get; init; }

	[GraphQLName("currencyId")]
	public required int CurrencyId { get; init; }

	[GraphQLName("transactionDateTime")]
	public required DateTime TransactionDateTime { get; init; }

	[GraphQLName("forcePayment")]
	public bool? ForcePayment { get; init; }
}
