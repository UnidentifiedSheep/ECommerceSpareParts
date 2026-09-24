namespace Notification.Channels.Email;

public sealed record EmailMessage(string Subject, string To, string Body);
