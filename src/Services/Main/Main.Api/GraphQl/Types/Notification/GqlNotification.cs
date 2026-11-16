using HotChocolate;
using Main.Modules.Notifications.Application.Dtos;

namespace Main.Api.GraphQl.Types.Notification;

[GraphQLName("Notification")]
public record GqlNotification(
	[property: GraphQLIgnore]
	InAppNotificationDto NotificationDto)
{
	[GraphQLName("id")]
	public int Id => NotificationDto.Id;

	[GraphQLName("userId")]
	public Guid UserId => NotificationDto.UserId;

	[GraphQLName("text")]
	public string Text => NotificationDto.Text;

	[GraphQLName("createdAt")]
	public DateTime CreateAt => NotificationDto.CreateAt;

	[GraphQLName("seenAt")]
	public DateTime? SeenAt => NotificationDto.SeenAt;
}
