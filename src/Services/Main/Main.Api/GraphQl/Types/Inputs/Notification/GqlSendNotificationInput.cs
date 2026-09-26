using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Notification;

[GraphQLName("SendNotificationInput")]
public record GqlSendNotificationInput
{
	[GraphQLName("userId")]
	public required Guid UserId { get; init; }

	[GraphQLName("message")]
	public required string Message { get; init; }
}
