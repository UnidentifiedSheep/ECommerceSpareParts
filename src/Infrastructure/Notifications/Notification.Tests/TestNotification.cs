using System.Globalization;
using System.Text.Json.Serialization;
using Notification.Core.Interfaces;

namespace Notification.Tests;

public class TestNotification : INotification<TestNotificationData>
{
	public const string Name = "TestNotification";
	public string SystemName => Name;
	public CultureInfo? SelectedCulture { get; }
	public TestNotificationData Model { get; }

	public TestNotification(
		TestNotificationData model,
		CultureInfo? selectedCulture = null)
	{
		Model = model;
		SelectedCulture = selectedCulture;
	}
}

public record TestNotificationData
{
	[JsonPropertyName("testInt")]
	public required int TestInt { get; init; }

	[JsonPropertyName("testString")]
	public required string TestString { get; init; }
}
