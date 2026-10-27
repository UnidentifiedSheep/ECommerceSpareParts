using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Transaction;

[GraphQLName("TransactionCursorValueInput")]
public readonly record struct GqlTransactionCursorValueInput
{
	[GraphQLName("id")]
	public required Guid Id { get; init; }

	[GraphQLName("transactionDate")]
	public required DateTime TransactionDate { get; init; }
}
