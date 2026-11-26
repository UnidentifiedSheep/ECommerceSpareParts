using Application.Common.Services.Events;
using Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Abstractions.Events;
using Pricing.Abstractions.Models;
using Pricing.Application.DomainEventHandler;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.PricingTests.DomainEventHandlers;

public class TrackPriceOfferDailyStatsHandlerTests : IntegrationTest
{
	private static readonly DateTime Morning = new(2026, 11, 25, 10, 0, 0, DateTimeKind.Utc);
	private static readonly DateTime Afternoon = new(2026, 11, 25, 15, 0, 0, DateTimeKind.Utc);

	public TrackPriceOfferDailyStatsHandlerTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private int ProductId => GetContext<ProductTestContext>().Products[0].Id;

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	private int CurrencyId => GetContext<CurrencyTestContext>().Currencies[0].Id;

	[Fact]
	public async Task Handle_GroupsOffersByPrice()
	{
		await HandleAsync(Event(Supplier.Armtek, Morning, Offer(1500m), Offer(1500m), Offer(1480m)));

		var stats = await GetStatsAsync();

		stats.Should().HaveCount(2);
		stats.Single(x => x.PurchasePrice == 1500m).OffersCount.Should().Be(2);
		stats.Single(x => x.PurchasePrice == 1480m).OffersCount.Should().Be(1);
		stats.Should().OnlyContain(x =>
			x.Day == new DateOnly(2026, 11, 25) &&
			x.Source == PriceOfferSource.Armtek &&
			x.ForStorage == StorageCode &&
			x.CurrencyId == CurrencyId &&
			x.FirstSeenAt == Morning &&
			x.LastSeenAt == Morning);
	}

	[Fact]
	public async Task Handle_SameDayEvents_AccumulateIntoExistingRow()
	{
		await HandleAsync(Event(Supplier.Armtek, Morning, Offer(1500m)));
		await HandleAsync(Event(Supplier.Armtek, Afternoon, Offer(1500m), Offer(1450m)));

		var stats = await GetStatsAsync();

		var same = stats.Single(x => x.PurchasePrice == 1500m);
		same.OffersCount.Should().Be(2);
		same.FirstSeenAt.Should().Be(Morning);
		same.LastSeenAt.Should().Be(Afternoon);
		stats.Single(x => x.PurchasePrice == 1450m).FirstSeenAt.Should().Be(Afternoon);
	}

	[Fact]
	public async Task Handle_EventsInOneBatch_MergedAndOrderIndependent()
	{
		await HandleAsync(
			Event(Supplier.Armtek, Afternoon, Offer(1500m)),
			Event(Supplier.Armtek, Morning, Offer(1500m)));

		var stat = (await GetStatsAsync()).Single();

		stat.OffersCount.Should().Be(2);
		stat.FirstSeenAt.Should().Be(Morning);
		stat.LastSeenAt.Should().Be(Afternoon);
	}

	[Fact]
	public async Task Handle_OtherDayAndSupplier_StoredSeparately()
	{
		await HandleAsync(Event(Supplier.Armtek, Morning, Offer(1500m)));
		await HandleAsync(Event(Supplier.Armtek, Morning.AddDays(1), Offer(1500m)));
		await HandleAsync(Event(Supplier.Tmtr, Morning, Offer(1500m)));

		var stats = await GetStatsAsync();

		stats.Should().HaveCount(3);
		stats.Should().OnlyContain(x => x.OffersCount == 1);
	}

	private Task HandleAsync(params SupplierOffersRefreshedEvent[] events)
		=> ActivatorUtilities.CreateInstance<TrackPriceOfferDailyStatsHandler>(Scope.ServiceProvider)
			.Handle(new Batch<SupplierOffersRefreshedEvent>(events), CancellationToken);

	private SupplierOffersRefreshedEvent Event(Supplier supplier, DateTime seenAt, params SupplierOfferSnapshot[] offers)
		=> new(supplier, StorageCode, seenAt, offers);

	private SupplierOfferSnapshot Offer(decimal price)
		=> new(ProductId, CurrencyId, price, 3, null, 95);

	private Task<List<PriceOfferDailyStat>> GetStatsAsync()
		=> Context.Set<PriceOfferDailyStat>().AsNoTracking().ToListAsync(CancellationToken);
}
