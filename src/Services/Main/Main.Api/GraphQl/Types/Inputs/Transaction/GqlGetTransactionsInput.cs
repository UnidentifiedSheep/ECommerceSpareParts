using Enums;
using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Transaction;

[GraphQLName("GetTransactionsInput")]
public sealed record GqlGetTransactionsInput
{
	[GraphQLName("cursor")]
	public required GqlCursor<GqlTransactionCursorValueInput?> Cursor { get; init; }

	[GraphQLName("rangeStart")]
	public DateTime? RangeStart { get; init; }

	[GraphQLName("rangeEnd")]
	public DateTime? RangeEnd { get; init; }

	[GraphQLName("currencyId")]
	public int? CurrencyId { get; init; }

	[GraphQLName("senderId")]
	public Guid? SenderId { get; init; }

	[GraphQLName("receiverId")]
	public Guid? ReceiverId { get; init; }

	[GraphQLName("logicalOperation")]
	public LogicalOperation? LogicalOperation { get; init; }

	[GraphQLName("skipReversed")]
	public bool? SkipReversed { get; init; }
}
