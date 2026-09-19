using Application.Common.Interfaces.Settings;
using Contracts.Settings;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Settings;
using Tests.Extensions;
using Tests.Integration;
using Tests.Persistence.Entities;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Settings;

public sealed class SettingsServicePersistenceTests(CombinedContainerFixture fixture)
	: CommonLayerIntegrationTest(fixture)
{
	private ISettingsService SettingsService => Scope.ServiceProvider.GetRequiredService<ISettingsService>();

	private ISettingsContainer SettingsContainer =>
		Scope.ServiceProvider.GetRequiredService<ISettingsContainer>();

	private MessageBrokerStub MessageBroker => Scope.ServiceProvider.GetRequiredService<MessageBrokerStub>();

	[Fact]
	public async Task LoadAsync_EmptyDatabase_MarksContainerAsLoaded()
	{
		await SettingsService.LoadAsync(CancellationToken);

		SettingsContainer.Loaded.Should().BeTrue();
		SettingsContainer.TryGet<TestSetting>(out _).Should().BeFalse();
	}

	[Fact]
	public async Task LoadAsync_PersistedSetting_LoadsMaterializedRuntimeType()
	{
		await new TestSettingBuilder(Faker).WithValue(42).BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		await SettingsService.LoadAsync(CancellationToken);

		var loaded = SettingsContainer.Get<TestSetting>();
		loaded.Should().BeOfType<TestSetting>();
		loaded.Data.Value.Should().Be(42);
		(await Context.Set<TestSetting>().AsNoTracking().SingleAsync(CancellationToken))
			.Should()
			.BeOfType<TestSetting>();
	}

	[Fact]
	public async Task SetSetting_MissingSetting_InsertsAndCachesIt()
	{
		var setting = new TestSettingBuilder(Faker).WithValue(10).Build();

		await SettingsService.SetSetting(setting, CancellationToken);

		var persisted = await Context.Set<TestSetting>().AsNoTracking().SingleAsync(CancellationToken);
		persisted.Data.Value.Should().Be(10);
		SettingsContainer.Get<TestSetting>().Should().BeSameAs(setting);
		AssertPublishedEvent(10);
	}

	[Fact]
	public async Task SetSetting_ExistingSetting_UpdatesWithoutDuplicateAndCachesInput()
	{
		await new TestSettingBuilder(Faker).WithValue(10).BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();
		var replacement = new TestSettingBuilder(Faker).WithValue(20).Build();

		await SettingsService.SetSetting(replacement, CancellationToken);

		var persisted = await Context.Set<TestSetting>().AsNoTracking().SingleAsync(CancellationToken);
		persisted.Data.Value.Should().Be(20);
		SettingsContainer.Get<TestSetting>().Should().BeSameAs(replacement);
		AssertPublishedEvent(20);
	}

	[Fact]
	public async Task GetOrDefault_CachedSetting_ReturnsItWithoutDatabaseAccess()
	{
		var cached = new TestSettingBuilder(Faker).WithValue(30).Build();
		SettingsContainer.Set(cached);

		var result = await SettingsService.GetOrDefault<TestSetting>(CancellationToken);

		result.Should().BeSameAs(cached);
		(await Context.Set<TestSetting>().CountAsync(CancellationToken)).Should().Be(0);
		MessageBroker.PublishedMessages.Should().BeEmpty();
	}

	[Fact]
	public async Task GetOrDefault_PersistedSetting_LoadsAndCachesIt()
	{
		await new TestSettingBuilder(Faker).WithValue(40).BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();

		var result = await SettingsService.GetOrDefault<TestSetting>(CancellationToken);

		result.Data.Value.Should().Be(40);
		SettingsContainer.Get<TestSetting>().Should().BeSameAs(result);
		MessageBroker.PublishedMessages.Should().BeEmpty();
	}

	[Fact]
	public async Task GetOrDefault_MissingSetting_PersistsCachesAndPublishesDefault()
	{
		var result = await SettingsService.GetOrDefault<TestSetting>(CancellationToken);

		result.Data.Should().Be(TestSetting.Default.Data);
		var persisted = await Context.Set<TestSetting>().AsNoTracking().SingleAsync(CancellationToken);
		persisted.Data.Should().Be(TestSetting.Default.Data);
		SettingsContainer.Get<TestSetting>().Should().BeSameAs(result);
		AssertPublishedEvent(TestSetting.Default.Data.Value);
	}

	private void AssertPublishedEvent(int expectedValue)
	{
		var @event = MessageBroker
			.PublishedMessagesOfType<SettingUpdatedEvent>()
			.Should()
			.ContainSingle()
			.Which;
		@event.Key.Should().Be(TestSetting.SettingName);
		@event.Value.Should().Be($"{{\"Value\":{expectedValue}}}");
		@event.ChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
	}
}
