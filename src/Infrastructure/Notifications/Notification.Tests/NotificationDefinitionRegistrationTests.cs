using Microsoft.Extensions.DependencyInjection;
using NamedObject.Core.Interfaces;
using Notification.Core;
using Notification.Core.Interfaces;
using Notification.Core.Interfaces.Notification;
using Notification.Extensions;

namespace Notification.Tests;

public class NotificationDefinitionRegistrationTests
{
	[Fact]
	public void SimpleNotification_WithStringModel_RoundTripsThroughDefinition()
	{
		var definition = new NotificationDefinitionBase<SimpleTestNotification, string>(
			"SimpleTestNotification",
			new NotificationSerializer(),
			model => new SimpleTestNotification(model));
		var original = new SimpleTestNotification("Hello, Alice");

		var entity = definition.ToEntity(Guid.NewGuid(), original);
		var restored = definition.FromEntity(entity);

		Assert.Equal(original.Model, restored.Model);
		Assert.Equal(entity.Model, new NotificationSerializer().Serialize(restored));
		Assert.IsAssignableFrom<ISimpleNotification>(restored);
	}

	[Fact]
	public void RegisteredNotification_RoundTripsThroughRegistry()
	{
		var services = CreateServices();
		services.AddNotification<TestNotification, TestNotificationData>(
			TestNotification.Name,
			model => new TestNotification(model));

		using var provider = services.BuildServiceProvider(validateScopes: true);
		using var scope = provider.CreateScope();
		var registry = scope.ServiceProvider
			.GetRequiredService<INamedObjectRegistry<INotificationDefinition>>();
		var definition = registry.GetBySystemName("testnotification");
		var userId = Guid.NewGuid();
		var original = new TestNotification(
			new TestNotificationData { TestInt = 42, TestString = "message" });

		var entity = definition.ToEntity(userId, original);
		var restored = Assert.IsType<TestNotification>(definition.FromEntity(entity));

		Assert.IsType<NotificationDefinitionBase<TestNotification, TestNotificationData>>(definition);
		Assert.Equal(userId, entity.UserId);
		Assert.Equal("TestNotification", entity.NotificationSystemName);
		Assert.Equal(42, restored.Model.TestInt);
		Assert.Equal("message", restored.Model.TestString);
	}

	[Fact]
	public void RegisteredNotification_IsSingletonAcrossScopes()
	{
		var services = CreateServices();
		services.AddNotification<TestNotification, TestNotificationData>(
			"TestNotification",
			model => new TestNotification(model));
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
			model => new TestNotification(model));
		services.AddNotification<OtherNotification, TestNotificationData>(
			"OtherNotification",
			model => new OtherNotification(model));

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
			model => new TestNotification(model));
		services.AddNotification<OtherNotification, TestNotificationData>(
			"Duplicate",
			model => new OtherNotification(model));

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

	private sealed record OtherNotification(TestNotificationData Model) : INotification<TestNotificationData>
	{
		public string SystemName => "OtherNotification";
	}

	private sealed record SimpleTestNotification(string Model) : ISimpleNotification
	{
		public string SystemName => "SimpleTestNotification";
	}
}
