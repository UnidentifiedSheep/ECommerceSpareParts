using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.Persistence;

public sealed class PriceOfferDailyStatRepositoryTests : IntegrationTest
{
	private static readonly DateTime Morning = new(2026, 11, 25, 10, 0, 0, DateTimeKind.Utc);
	private static readonly DateTime Noon = new(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
	private static readonly DateTime Afternoon = new(2026, 11, 25, 15, 0, 0, DateTimeKind.Utc);

	public PriceOfferDailyStatRepositoryTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private IPriceOfferDailyStatRepository Repository
		=> Scope.ServiceProvider.GetRequiredService<IPriceOfferDailyStatRepository>();

	private int ProductId => GetContext<ProductTestContext>().Products[0].Id;

	private int OtherProductId => GetContext<ProductTestContext>().Products[1].Id;

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	private int CurrencyId => GetContext<CurrencyTestContext>().Currencies[0].Id;

	private int OtherCurrencyId => GetContext<CurrencyTestContext>().Currencies[1].Id;

	[Fact]
	public async Task Upsert_NewRow_PersistsEveryProperty()
	{
		var stat = PriceOfferDailyStat.Merge(
			Stat(Morning, 1500.25m, offersCount: 2),
			Stat(Afternoon, 1500.25m, offersCount: 3));

		var properties = typeof(PriceOfferDailyStat)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		properties.Should().OnlyContain(
			property => !IsDefault(property.GetValue(stat)),
			"every property must be filled so that a column forgotten in UpsertAsync is detected");

		await Repository.UpsertAsync([stat], CancellationToken);

		var saved = (await GetStatsAsync()).Should().ContainSingle().Subject;
		saved.Should().BeEquivalentTo(stat);
	}

	[Fact]
	public async Task Upsert_ExistingRow_AccumulatesCountAndWidensSeenRange()
	{
		await Repository.UpsertAsync([Stat(Noon, 1500m, offersCount: 1)], CancellationToken);

		await Repository.UpsertAsync(
			[PriceOfferDailyStat.Merge(Stat(Morning, 1500m, offersCount: 2), Stat(Afternoon, 1500m, offersCount: 1))],
			CancellationToken);

		var saved = (await GetStatsAsync()).Should().ContainSingle().Subject;
		saved.OffersCount.Should().Be(4);
		saved.FirstSeenAt.Should().Be(Morning);
		saved.LastSeenAt.Should().Be(Afternoon);
	}

	[Fact]
	public async Task Upsert_ExistingRow_InnerRangeKeepsBounds()
	{
		await Repository.UpsertAsync(
			[PriceOfferDailyStat.Merge(Stat(Morning, 1500m, offersCount: 1), Stat(Afternoon, 1500m, offersCount: 1))],
			CancellationToken);

		await Repository.UpsertAsync([Stat(Noon, 1500m, offersCount: 1)], CancellationToken);

		var saved = (await GetStatsAsync()).Should().ContainSingle().Subject;
		saved.OffersCount.Should().Be(3);
		saved.FirstSeenAt.Should().Be(Morning);
		saved.LastSeenAt.Should().Be(Afternoon);
	}

	[Fact]
	public async Task Upsert_DifferentKeyParts_StoredAsSeparateRows()
	{
		await Repository.UpsertAsync(
			[
				Stat(Morning, 1500m, offersCount: 1),
				Stat(Morning, 1480m, offersCount: 1),
				Stat(Morning.AddDays(1), 1500m, offersCount: 1),
				Stat(Morning, 1500m, offersCount: 1, source: PriceOfferSource.Tmtr),
				Stat(Morning, 1500m, offersCount: 1, productId: OtherProductId),
				Stat(Morning, 1500m, offersCount: 1, currencyId: OtherCurrencyId)
			],
			CancellationToken);

		var stats = await GetStatsAsync();

		stats.Should().HaveCount(6);
		stats.Should().OnlyContain(x => x.OffersCount == 1);
	}

	[Fact]
	public async Task Upsert_Empty_DoesNothing()
	{
		await Repository.UpsertAsync([], CancellationToken);

		(await GetStatsAsync()).Should().BeEmpty();
	}

	private PriceOfferDailyStat Stat(
		DateTime seenAt,
		decimal price,
		int offersCount,
		PriceOfferSource source = PriceOfferSource.Armtek,
		int? productId = null,
		int? currencyId = null)
		=> PriceOfferDailyStat.Create(
			productId ?? ProductId,
			source,
			StorageCode,
			currencyId ?? CurrencyId,
			price,
			offersCount,
			seenAt);

	private Task<List<PriceOfferDailyStat>> GetStatsAsync()
		=> Context.Set<PriceOfferDailyStat>().AsNoTracking().ToListAsync(CancellationToken);

	private static bool IsDefault(object? value)
		=> value is null || (value.GetType().IsValueType && value.Equals(Activator.CreateInstance(value.GetType())));
}
