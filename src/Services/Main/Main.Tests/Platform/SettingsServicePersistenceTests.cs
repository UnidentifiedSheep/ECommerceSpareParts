using Application.Common.Interfaces.Settings;
using Contracts.Settings;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Main.Entities.Settings.Supplier;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.Platform;

public sealed class SettingsServicePersistenceTests(CombinedContainerFixture fixture)
	: IntegrationTest(fixture)
{
	private ISettingsService SettingsService => Scope.ServiceProvider.GetRequiredService<ISettingsService>();

	private ISettingsContainer SettingsContainer
		=> Scope.ServiceProvider.GetRequiredService<ISettingsContainer>();

	private MessageBrokerStub MessageBroker => (MessageBrokerStub)Scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

	[Fact]
	public async Task LoadAsync_EmptyDatabase_MarksContainerAsLoaded()
	{
		await SettingsService.LoadAsync(CancellationToken);

		SettingsContainer.Loaded.Should().BeTrue();
		SettingsContainer.TryGet<TmtrSupplierSetting>(out _).Should().BeFalse();
	}

	[Fact]
	public async Task LoadAsync_PersistedSetting_LoadsMaterializedRuntimeType()
	{
		await AddAsync(42);
		Context.ChangeTracker.Clear();

		await SettingsService.LoadAsync(CancellationToken);

		var loaded = SettingsContainer.Get<TmtrSupplierSetting>();
		loaded.Should().BeOfType<TmtrSupplierSetting>();
		loaded.Data.BaseUrl.Should().Be(Url(42));
		(await Context.Set<TmtrSupplierSetting>().AsNoTracking().SingleAsync(CancellationToken))
			.Should()
			.BeOfType<TmtrSupplierSetting>();
	}

	[Fact]
	public async Task SetSetting_MissingSetting_InsertsAndCachesIt()
	{
		var setting = Create(10);

		await SettingsService.SetSetting(setting, CancellationToken);

		var persisted = await Context.Set<TmtrSupplierSetting>().AsNoTracking().SingleAsync(CancellationToken);
		persisted.Data.BaseUrl.Should().Be(Url(10));
		SettingsContainer.Get<TmtrSupplierSetting>().Should().BeSameAs(setting);
		AssertPublishedEvent(Create(10).Json);
	}

	[Fact]
	public async Task SetSetting_ExistingSetting_UpdatesWithoutDuplicateAndCachesInput()
	{
		await AddAsync(10);
		Context.ChangeTracker.Clear();
		var replacement = Create(20);

		await SettingsService.SetSetting(replacement, CancellationToken);

		var persisted = await Context.Set<TmtrSupplierSetting>().AsNoTracking().SingleAsync(CancellationToken);
		persisted.Data.BaseUrl.Should().Be(Url(20));
		SettingsContainer.Get<TmtrSupplierSetting>().Should().BeSameAs(replacement);
		AssertPublishedEvent(Create(20).Json);
	}

	[Fact]
	public async Task GetOrDefault_CachedSetting_ReturnsItWithoutDatabaseAccess()
	{
		var cached = Create(30);
		SettingsContainer.Set(cached);

		var result = await SettingsService.GetOrDefault<TmtrSupplierSetting>(CancellationToken);

		result.Should().BeSameAs(cached);
		(await Context.Set<TmtrSupplierSetting>().CountAsync(CancellationToken)).Should().Be(0);
		TmtrEvents.Should().BeEmpty();
	}

	[Fact]
	public async Task GetOrDefault_PersistedSetting_LoadsAndCachesIt()
	{
		await AddAsync(40);
		Context.ChangeTracker.Clear();

		var result = await SettingsService.GetOrDefault<TmtrSupplierSetting>(CancellationToken);

		result.Data.BaseUrl.Should().Be(Url(40));
		SettingsContainer.Get<TmtrSupplierSetting>().Should().BeSameAs(result);
		TmtrEvents.Should().BeEmpty();
	}

	[Fact]
	public async Task GetOrDefault_MissingSetting_PersistsCachesAndPublishesDefault()
	{
		var result = await SettingsService.GetOrDefault<TmtrSupplierSetting>(CancellationToken);

		result.Data.Should().Be(TmtrSupplierSetting.Default.Data);
		var persisted = await Context.Set<TmtrSupplierSetting>().AsNoTracking().SingleAsync(CancellationToken);
		persisted.Data.Should().Be(TmtrSupplierSetting.Default.Data);
		SettingsContainer.Get<TmtrSupplierSetting>().Should().BeSameAs(result);
		AssertPublishedEvent(TmtrSupplierSetting.Default.Json);
	}

	[Fact]
	public async Task GetOrDefault_DirectRead_BypassesCachedAndTrackedValuesWithoutUpdatingCache()
	{
		var stale = await AddAsync(10);
		SettingsContainer.Set(stale);
		var replacement = Create(20);
		await Context.Set<TmtrSupplierSetting>()
			.ExecuteUpdateAsync(
				update => update.SetProperty(x => x.Json, replacement.Json),
				CancellationToken);

		var result = await SettingsService.GetOrDefault<TmtrSupplierSetting>(true, CancellationToken);

		result.Data.BaseUrl.Should().Be(Url(20));
		result.Should().NotBeSameAs(stale);
		stale.Data.BaseUrl.Should().Be(Url(10));
		SettingsContainer.Get<TmtrSupplierSetting>().Should().BeSameAs(stale);
		TmtrEvents.Should().BeEmpty();
	}

	[Fact]
	public async Task GetOrDefault_DirectRead_WhenSettingExistsOnlyInCache_ReturnsDefault()
	{
		var cached = Create(30);
		SettingsContainer.Set(cached);

		var result = await SettingsService.GetOrDefault<TmtrSupplierSetting>(true, CancellationToken);

		result.Data.Should().Be(TmtrSupplierSetting.Default.Data);
		SettingsContainer.Get<TmtrSupplierSetting>().Should().BeSameAs(cached);
		(await Context.Set<TmtrSupplierSetting>().CountAsync(CancellationToken)).Should().Be(0);
		TmtrEvents.Should().BeEmpty();
	}

	[Fact]
	public async Task GetOrDefault_DirectRead_MissingSetting_ReturnsDefaultWithoutSavingOrCaching()
	{
		var result = await SettingsService.GetOrDefault<TmtrSupplierSetting>(true, CancellationToken);

		result.Data.Should().Be(TmtrSupplierSetting.Default.Data);
		SettingsContainer.TryGet<TmtrSupplierSetting>(out _).Should().BeFalse();
		(await Context.Set<TmtrSupplierSetting>().CountAsync(CancellationToken)).Should().Be(0);
		TmtrEvents.Should().BeEmpty();
	}

	private List<SettingUpdatedEvent> TmtrEvents
		=> MessageBroker
			.PublishedMessagesOfType<SettingUpdatedEvent>()
			.Where(item => item.Key == TmtrSupplierSetting.SettingName)
			.ToList();

	private static string Url(int value) => $"https://app-{value}.example";

	private static TmtrSupplierSetting Create(int value)
		=> new(new TmtrSupplierSettingData { BaseUrl = Url(value) });

	private async Task<TmtrSupplierSetting> AddAsync(int value)
	{
		var setting = Create(value);
		await Context.AddAsync(setting, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		return setting;
	}

	private void AssertPublishedEvent(string expectedJson)
	{
		var @event = TmtrEvents
			.Should()
			.ContainSingle()
			.Which;
		@event.Key.Should().Be(TmtrSupplierSetting.SettingName);
		@event.Value.Should().Be(expectedJson);
		@event.ChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
	}
}
