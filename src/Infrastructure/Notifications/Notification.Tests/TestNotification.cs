using System.Text.Json.Serialization;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;

namespace Notification.Tests;

public class TestNotification : INotification<TestNotificationData>
{
	public const string Name = "TestNotification";
	public string SystemName => Name;
	public TestNotificationData Model { get; }

	public TestNotification(TestNotificationData model)
	{
		Model = model;
	}
}

public record TestNotificationData
{
	[JsonPropertyName("testInt")]
	public required int TestInt { get; init; }

	[JsonPropertyName("testString")]
	public required string TestString { get; init; }
}
