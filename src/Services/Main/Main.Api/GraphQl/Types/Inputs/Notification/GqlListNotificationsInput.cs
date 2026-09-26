using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Notification;

[GraphQLName("ListNotificationsInput")]
public record GqlListNotificationsInput
{
	[GraphQLName("cursor")]
	public required GqlCursor<DateTime?> Cursor { get; init; }
}
