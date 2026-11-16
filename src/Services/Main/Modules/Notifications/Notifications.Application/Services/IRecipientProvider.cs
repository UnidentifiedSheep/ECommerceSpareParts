using Notification.Core.Interfaces.Recipient;

namespace Main.Modules.Notifications.Application.Services;

public interface IRecipientProvider : IRecipientResolver
{
	Task InvalidateUserRecipients(Guid userId);

	Task InvalidateUsersRecipients(IEnumerable<Guid> userIds);
}
