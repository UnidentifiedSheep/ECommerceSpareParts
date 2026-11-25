using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public sealed class PriceOfferRefreshStateRepositoryTests : IntegrationTest
{
	private const string StorageCode = "MAIN";
	private static readonly DateTime Morning = new(2026, 11, 25, 10, 0, 0, DateTimeKind.Utc);
	private static readonly DateTime Afternoon = new(2026, 11, 25, 15, 0, 0, DateTimeKind.Utc);

	public PriceOfferRefreshStateRepositoryTests(CombinedContainerFixture fixture) : base(fixture)
	{
	}

	private IPriceOfferRefreshStateRepository Repository
		=> Scope.ServiceProvider.GetRequiredService<IPriceOfferRefreshStateRepository>();

	[Fact]
	public async Task Upsert_NewState_PersistsEveryPropertyAndIsAllowed()
	{
		var state = State(7, PriceOfferSource.Tmtr, Morning, 12);

		var properties = typeof(PriceOfferRefreshState)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		properties.Should().OnlyContain(
			property => !IsDefault(property.GetValue(state)),
			"every property must be filled so that a column forgotten in UpsertStatesAsync is detected");

		var allowed = await Repository.UpsertStatesAsync([state], CancellationToken);

		allowed.Should().BeEquivalentTo([state.GetId()]);
		var saved = (await GetStatesAsync()).Should().ContainSingle().Subject;
		saved.Should().BeEquivalentTo(state);
	}

	[Fact]
	public async Task Upsert_NewerData_UpdatesAndIsAllowed()
	{
		await Repository.UpsertStatesAsync([State(1, PriceOfferSource.Armtek, Morning, 3)], CancellationToken);

		var allowed = await Repository.UpsertStatesAsync(
			[State(1, PriceOfferSource.Armtek, Afternoon, 5)],
			CancellationToken);

		allowed.Should().ContainSingle();
		var saved = (await GetStatesAsync()).Single();
		saved.LastOffersUpdatedAt.Should().Be(Afternoon);
		saved.LastOffersCount.Should().Be(5);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task Upsert_SameOrOlderData_NotAllowedAndUnchanged(int hoursShift)
	{
		await Repository.UpsertStatesAsync([State(1, PriceOfferSource.Armtek, Afternoon, 3)], CancellationToken);

		var allowed = await Repository.UpsertStatesAsync(
			[State(1, PriceOfferSource.Armtek, Afternoon.AddHours(hoursShift), 9)],
			CancellationToken);

		allowed.Should().BeEmpty();
		var saved = (await GetStatesAsync()).Single();
		saved.LastOffersUpdatedAt.Should().Be(Afternoon);
		saved.LastOffersCount.Should().Be(3);
	}

	[Fact]
	public async Task Upsert_ExistingWithoutUpdateTime_IsAllowed()
	{
		await Repository.UpsertStatesAsync(
			[PriceOfferRefreshState.Create(1, PriceOfferSource.Armtek, StorageCode)],
			CancellationToken);

		var allowed = await Repository.UpsertStatesAsync(
			[State(1, PriceOfferSource.Armtek, Morning, 2)],
			CancellationToken);

		allowed.Should().ContainSingle();
		(await GetStatesAsync()).Single().LastOffersUpdatedAt.Should().Be(Morning);
	}

	[Fact]
	public async Task Upsert_MixedBatch_ReturnsOnlyAllowedKeys()
	{
		await Repository.UpsertStatesAsync(
			[
				State(1, PriceOfferSource.Armtek, Afternoon, 1),
				State(2, PriceOfferSource.Armtek, Morning, 1)
			],
			CancellationToken);

		var stale = State(1, PriceOfferSource.Armtek, Morning, 4);
		var newer = State(2, PriceOfferSource.Armtek, Afternoon, 4);
		var created = State(3, PriceOfferSource.Armtek, Morning, 4);
		var otherSource = State(1, PriceOfferSource.Tmtr, Morning, 4);

		var allowed = await Repository.UpsertStatesAsync([stale, newer, created, otherSource], CancellationToken);

		allowed.Should().BeEquivalentTo([newer.GetId(), created.GetId(), otherSource.GetId()]);
		(await GetStatesAsync()).Should().HaveCount(4);
	}

	[Fact]
	public async Task Upsert_DuplicateKeysInBatch_NewestWins()
	{
		var allowed = await Repository.UpsertStatesAsync(
			[
				State(1, PriceOfferSource.Armtek, Morning, 1),
				State(1, PriceOfferSource.Armtek, Afternoon, 2)
			],
			CancellationToken);

		allowed.Should().ContainSingle();
		var saved = (await GetStatesAsync()).Single();
		saved.LastOffersUpdatedAt.Should().Be(Afternoon);
		saved.LastOffersCount.Should().Be(2);
	}

	[Fact]
	public async Task Upsert_Empty_ReturnsEmpty()
	{
		var allowed = await Repository.UpsertStatesAsync([], CancellationToken);

		allowed.Should().BeEmpty();
		(await GetStatesAsync()).Should().BeEmpty();
	}

	private static PriceOfferRefreshState State(
		int productId,
		PriceOfferSource source,
		DateTime updatedAt,
		int offersCount)
	{
		var state = PriceOfferRefreshState.Create(productId, source, StorageCode);
		state.OffersUpdated(updatedAt, offersCount);
		return state;
	}

	private Task<List<PriceOfferRefreshState>> GetStatesAsync()
		=> Context.Set<PriceOfferRefreshState>().AsNoTracking().ToListAsync(CancellationToken);

	private static bool IsDefault(object? value)
		=> value is null || (value.GetType().IsValueType && value.Equals(Activator.CreateInstance(value.GetType())));
}
