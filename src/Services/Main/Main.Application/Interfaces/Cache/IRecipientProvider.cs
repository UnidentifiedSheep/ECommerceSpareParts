using Notification.Core.Interfaces.Recipient;

namespace Main.Application.Interfaces.Cache;

public interface IRecipientProvider : IRecipientResolver
{
	Task InvalidateUserRecipients(Guid userId);

	Task InvalidateUsersRecipients(IEnumerable<Guid> userIds);
}
