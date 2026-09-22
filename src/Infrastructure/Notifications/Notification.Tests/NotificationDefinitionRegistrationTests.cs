using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using NamedObject.Core.Interfaces;
using Notification.Core;
using Notification.Core.Interfaces;

namespace Notification.Tests;

public class NotificationDefinitionRegistrationTests
{
	[Fact]
	public void RegisteredNotification_RoundTripsThroughRegistry()
	{
		var services = CreateServices();
		services.AddNotification<TestNotification, TestNotificationData>(
			"TestNotification",
			(model, culture) => new TestNotification(model, culture));

		using var provider = services.BuildServiceProvider(validateScopes: true);
		using var scope = provider.CreateScope();
		var registry = scope.ServiceProvider
			.GetRequiredService<INamedObjectRegistry<INotificationDefinition>>();
		var definition = registry.GetBySystemName("testnotification");
		var userId = Guid.NewGuid();
		var original = new TestNotification(
			new TestNotificationData { TestInt = 42, TestString = "message" },
			CultureInfo.GetCultureInfo("ru-RU"));

		var entity = definition.ToEntity(userId, original);
		var restored = Assert.IsType<TestNotification>(definition.FromEntity(entity));

		Assert.IsType<NotificationDefinitionBase<TestNotification, TestNotificationData>>(definition);
		Assert.Equal(userId, entity.UserId);
		Assert.Equal("TestNotification", entity.NotificationSystemName);
		Assert.Equal(42, restored.Model.TestInt);
		Assert.Equal("message", restored.Model.TestString);
		Assert.Equal(original.SelectedCulture, restored.SelectedCulture);
	}

	[Fact]
	public void RegisteredNotification_IsSingletonAcrossScopes()
	{
		var services = CreateServices();
		services.AddNotification<TestNotification, TestNotificationData>(
			"TestNotification",
			(model, culture) => new TestNotification(model, culture));
		Assert.Equal(ServiceLifetime.Singleton, Assert.Single(services, descriptor =>
			descriptor.ServiceType == typeof(INotificationDefinition)).Lifetime);

		using var provider = services.BuildServiceProvider(validateScopes: true);
		using var firstScope = provider.CreateScope();
		using var secondScope = provider.CreateScope();
		var first = firstScope.ServiceProvider
			.GetRequiredService<INamedObjectRegistry<INotificationDefinition>>()
			.GetBySystemName("TestNotification");
		var second = secondScope.ServiceProvider
			.GetRequiredService<INamedObjectRegistry<INotificationDefinition>>()
			.GetBySystemName("TestNotification");

		Assert.Same(first, second);
	}

	[Fact]
	public void MultipleNotifications_AreAvailableInRegistry()
	{
		var services = CreateServices();
		services.AddNotification<TestNotification, TestNotificationData>(
			"TestNotification",
			(model, culture) => new TestNotification(model, culture));
		services.AddNotification<OtherNotification, TestNotificationData>(
			"OtherNotification",
			(model, culture) => new OtherNotification(model, culture));

		using var provider = services.BuildServiceProvider(validateScopes: true);
		using var scope = provider.CreateScope();
		var registry = scope.ServiceProvider
			.GetRequiredService<INamedObjectRegistry<INotificationDefinition>>();

		Assert.Equal(2, registry.All.Count);
		Assert.IsType<NotificationDefinitionBase<TestNotification, TestNotificationData>>(
			registry.GetBySystemName("TestNotification"));
		Assert.IsType<NotificationDefinitionBase<OtherNotification, TestNotificationData>>(
			registry.GetBySystemName("OtherNotification"));
	}

	[Fact]
	public void DuplicateSystemName_IsRejectedByRegistry()
	{
		var services = CreateServices();
		services.AddNotification<TestNotification, TestNotificationData>(
			"Duplicate",
			(model, culture) => new TestNotification(model, culture));
		services.AddNotification<OtherNotification, TestNotificationData>(
			"Duplicate",
			(model, culture) => new OtherNotification(model, culture));

		using var provider = services.BuildServiceProvider(validateScopes: true);
		using var scope = provider.CreateScope();

		Assert.Throws<ArgumentException>(() => scope.ServiceProvider
			.GetRequiredService<INamedObjectRegistry<INotificationDefinition>>());
	}

	private static ServiceCollection CreateServices()
	{
		var services = new ServiceCollection();
		services.AddSingleton<INotificationSerializer, NotificationSerializer>();
		return services;
	}

	private sealed record OtherNotification(
		TestNotificationData Model,
		CultureInfo? SelectedCulture) : INotification<TestNotificationData>
	{
		public string SystemName => "OtherNotification";
	}
}
