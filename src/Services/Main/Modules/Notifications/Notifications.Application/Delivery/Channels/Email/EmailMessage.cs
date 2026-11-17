namespace Main.Modules.Notifications.Application.Delivery.Channels.Email;

public sealed record EmailMessage(string Title, string To, string Body);
