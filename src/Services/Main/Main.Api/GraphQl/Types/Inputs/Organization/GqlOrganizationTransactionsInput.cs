using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Organization;

[GraphQLName("OrganizationTransactionsInput")]
public sealed record GqlOrganizationTransactionsInput
{
	[GraphQLName("cursor")]
	public required GqlCursor<GqlTransactionCursorValueInput?> Cursor { get; init; }

	[GraphQLName("rangeStart")]
	public DateTime? RangeStart { get; init; }

	[GraphQLName("rangeEnd")]
	public DateTime? RangeEnd { get; init; }

	[GraphQLName("currencyId")]
	public int? CurrencyId { get; init; }

	[GraphQLName("skipReversed")]
	public bool? SkipReversed { get; init; }
}

[GraphQLName("TransactionCursorValueInput")]
public readonly record struct GqlTransactionCursorValueInput
{
	[GraphQLName("id")]
	public required Guid Id { get; init; }

	[GraphQLName("transactionDate")]
	public required DateTime TransactionDate { get; init; }
}
