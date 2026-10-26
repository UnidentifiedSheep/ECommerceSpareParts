using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Transaction;

[GraphQLName("ReverseTransactionInput")]
public sealed record GqlReverseTransactionInput
{
	[GraphQLName("transactionId")]
	public required Guid TransactionId { get; init; }

	[GraphQLName("forcePayment")]
	public bool? ForcePayment { get; init; }
}
