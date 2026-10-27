using GraphQL.Common.Types;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Transaction;

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
