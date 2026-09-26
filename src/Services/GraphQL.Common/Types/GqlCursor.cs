using Abstractions.Models;

namespace GraphQL.Common.Types;

public record GqlCursor<TCursor>
{
	[GraphQLName("value")]
	public required TCursor Value { get; init; }

	[GraphQLName("size")]
	public required int Size { get; init; }

	public static implicit operator Cursor<TCursor>(GqlCursor<TCursor> cursor)
		=> new(cursor.Value, cursor.Size);
}
