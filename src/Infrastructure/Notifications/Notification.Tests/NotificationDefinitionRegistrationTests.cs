using System.Globalization;
using Locan.Core.LocalizableMessages;
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
	public void SimpleNotification_WithConcreteModel_RoundTripsThroughDefinition()
	{
		var definition = new NotificationDefinitionBase<SimpleTestNotification, SimpleTestMessage>(
			"SimpleTestNotification",
			new NotificationSerializer(),
			(model, culture) => new SimpleTestNotification(model, culture));
		var original = new SimpleTestNotification(
			new SimpleTestMessage().WithName("Alice"),
			CultureInfo.GetCultureInfo("ru-RU"));

		var entity = definition.ToEntity(Guid.NewGuid(), original);
		var restored = definition.FromEntity(entity);

		Assert.Equal(original.Model.MessageKey, restored.Model.MessageKey);
		Assert.Equal(entity.Model, new NotificationSerializer().Serialize(restored));
		Assert.Equal(original.SelectedCulture, restored.SelectedCulture);
		Assert.IsAssignableFrom<ISimpleNotification<Locan.Core.Interfaces.ILocalizableMessage>>(restored);
	}

	[Fact]
	public void LocalizableMessage_AsStoredModel_CannotBeDeserialized()
	{
		var serializer = new NotificationSerializer();
		var json = serializer.Serialize(new BaseMessageNotification(new LocalizableMessage("notification.test")));

		Assert.Throws<InvalidOperationException>(() => serializer.Deserialize<LocalizableMessage>(json));
	}

	[Fact]
	public void RegisteredNotification_RoundTripsThroughRegistry()
	{
		var services = CreateServices();
		services.AddNotification<TestNotification, TestNotificationData>(
			TestNotification.Name,
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

	private sealed record SimpleTestNotification(
		SimpleTestMessage Model,
		CultureInfo? SelectedCulture) : ISimpleNotification<SimpleTestMessage>
	{
		public string SystemName => "SimpleTestNotification";
	}

	private sealed class SimpleTestMessage() : LocalizableMessage("notification.test")
	{
		public SimpleTestMessage WithName(string name)
		{
			WithValue("Name", name);
			return this;
		}
	}

	private sealed record BaseMessageNotification(LocalizableMessage Model)
		: ISimpleNotification<LocalizableMessage>
	{
		public string SystemName => "BaseMessageNotification";
		public CultureInfo? SelectedCulture => null;
	}
}
