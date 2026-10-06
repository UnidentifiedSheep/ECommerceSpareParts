using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
	public required string DocumentUrl { get; init; }
	public required string Title { get; init; }
	public required string AsText { get; init; }

	public DocumentGeneratedNotificationData() { }

	[SetsRequiredMembers]
	public DocumentGeneratedNotificationData(
		ILocalizer localizer,
		CultureInfo culture,
		ILocalizableMessage documentName,
		ILocalizableMessage documentDescription,
		Guid requestId,
		string documentUrl)
	{
		RequestId = requestId;
		DocumentName = localizer.Get(documentName, culture);
		DocumentDescription = localizer.Get(documentDescription, culture);
		DocumentUrl = documentUrl;
		Title = localizer.Get(NotificationsDocumentGeneratedTitleMessage.Instance, culture);
		AsText = localizer.Get(
			NotificationsDocumentGeneratedTextMessage.Create(DocumentName, documentUrl), culture);
	}
}
