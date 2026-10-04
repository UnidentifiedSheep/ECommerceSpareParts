using System.Diagnostics.CodeAnalysis;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Main.Entities;
using Notification.Core.Interfaces.Notification;

namespace Main.Application.Notifications;

public sealed class DocumentGeneratedNotification(DocumentGeneratedNotificationData model)
	: INotification<DocumentGeneratedNotificationData>, ITextNotification
{
	public const string NotificationSystemName = "DocumentGenerated";

	public string SystemName => NotificationSystemName;

	public DocumentGeneratedNotificationData Model { get; } = model;
}

public sealed record DocumentGeneratedNotificationData : INotificationModel, INotificationModelWithTitle
{
	public required Guid RequestId { get; init; }
	public required string DocumentName { get; init; }
	public required string DocumentDescription { get; init; }
	public required string GeneratedFileLink { get; init; }
	public required string Title { get; init; }
	public required string AsText { get; init; }

	public DocumentGeneratedNotificationData() { }

	[SetsRequiredMembers]
	public DocumentGeneratedNotificationData(
		IContextualLocalizer localizer,
		ILocalizableMessage documentName,
		ILocalizableMessage documentDescription,
		Guid requestId,
		string generatedFileLink)
	{
		RequestId = requestId;
		DocumentName = localizer.Get(documentName);
		DocumentDescription = localizer.Get(documentDescription);
		GeneratedFileLink = generatedFileLink;
		Title = localizer.Get(NotificationsDocumentGeneratedTitleMessage.Instance);
		AsText = localizer.Get(
			NotificationsDocumentGeneratedTextMessage.Create(DocumentName, generatedFileLink));
	}
}
