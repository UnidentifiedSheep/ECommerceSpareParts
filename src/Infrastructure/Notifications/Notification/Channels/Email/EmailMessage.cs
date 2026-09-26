namespace Notification.Channels.Email;

public sealed record EmailMessage(string Title, string To, string Body);
