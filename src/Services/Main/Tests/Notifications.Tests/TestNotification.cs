using System.Text.Json.Serialization;
using Notification.Core.Interfaces.Notification;

namespace Notifications.Tests;

public class TestNotification : INotification<TestNotificationData>
{
	public const string Name = "TestNotification";

	public TestNotification(TestNotificationData model) => Model = model;
	public string SystemName => Name;
	public TestNotificationData Model { get; }
}

public record TestNotificationData : INotificationModel
{
	[JsonPropertyName("testInt")]
	public required int TestInt { get; init; }

	[JsonPropertyName("testString")]
	public required string TestString { get; init; }

	[JsonPropertyName("asText")]
	public string AsText { get; init; } = string.Empty;
}
