using Application.Common.Services.Events;
using Enums;
using FluentAssertions;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Statistics;
using Main.Modules.Logistics.Application.DomainEventHandlers.SupplierOffersRefreshed;
using Main.Modules.Logistics.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Abstractions.Events;
using Pricing.Abstractions.Models;
using Tests.DataBuilders.Logistics;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Storage;

namespace Tests.DomainEventHandlers.Logistics;

public sealed class TrackDeliveryDailyStatsHandlerTests : IntegrationTest
{
	private static readonly DateTime Morning = new(2026, 11, 25, 10, 0, 0, DateTimeKind.Utc);
	private static readonly DateTime Afternoon = new(2026, 11, 25, 15, 0, 0, DateTimeKind.Utc);

	public TrackDeliveryDailyStatsHandlerTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private int ProductId => GetContext<ProductTestContext>().Products[0].Id;

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	[Fact]
	public async Task Handle_DayWithSpread_StoresRowPerDeliveryDays()
	{
		await HandleAsync(
			Event(Morning,
				Offer(Morning.AddDays(1), 95, Morning.AddDays(2)),
				Offer(Morning.AddDays(1), 90),
				Offer(Morning.AddDays(3), 80, Morning.AddDays(4)),
				Offer(Morning.AddDays(90), 50)));
		await HandleAsync(
			Event(Afternoon,
				Offer(Afternoon.AddDays(1), 97, Afternoon.AddDays(2)),
				Offer(Afternoon.AddDays(3), 85, Afternoon.AddDays(3)),
				Offer(Afternoon.AddDays(60), 50)));

		var stats = (await GetStatsAsync()).ToDictionary(x => x.DeliveryDays);

		stats.Keys.Should().BeEquivalentTo([1, 3, 60, 90]);
		stats[1].Should().Match<DeliveryDailyStat>(x =>
			x.OffersCount == 3 && x.MaxProbability == 97 && x.MinGuaranteedDays == 2 &&
			x.FirstSeenAt == Morning && x.LastSeenAt == Afternoon);
		stats[3].Should().Match<DeliveryDailyStat>(x =>
			x.OffersCount == 2 && x.MaxProbability == 85 && x.MinGuaranteedDays == 3);
		stats[60].Should().Match<DeliveryDailyStat>(x =>
			x.OffersCount == 1 && x.MinGuaranteedDays == null && x.FirstSeenAt == Afternoon);
		stats[90].Should().Match<DeliveryDailyStat>(x =>
			x.OffersCount == 1 && x.LastSeenAt == Morning);
	}

	[Fact]
	public async Task Handle_WithSchedule_CountsReceivingDays()
	{
		await new DeliveryScheduleBuilder(Faker)
			.WithSupplier(Supplier.Armtek)
			.WithStorageCode(StorageCode)
			.WithDays(DeliveryDays.Tuesday | DeliveryDays.Thursday)
			.BuildAndAddToDb(Context, CancellationToken);
		var friday = new DateTime(2026, 11, 27, 10, 0, 0, DateTimeKind.Utc);

		await HandleAsync(Event(friday, Offer(friday.AddDays(3), 95, friday.AddDays(4))));

		var stat = (await GetStatsAsync()).Single();
		stat.DeliveryDays.Should().Be(1);
		stat.MinGuaranteedDays.Should().Be(1);
		stat.Day.Should().Be(new DateOnly(2026, 11, 27));
	}

	[Fact]
	public async Task Handle_EventsInOneBatch_Merged()
	{
		await HandleAsync(
			Event(Afternoon, Offer(Afternoon.AddDays(2), 70)),
			Event(Morning, Offer(Morning.AddDays(2), 90, Morning.AddDays(5))));

		var stat = (await GetStatsAsync()).Single();

		stat.OffersCount.Should().Be(2);
		stat.MaxProbability.Should().Be(90);
		stat.MinGuaranteedDays.Should().Be(5);
		stat.FirstSeenAt.Should().Be(Morning);
		stat.LastSeenAt.Should().Be(Afternoon);
	}

	private async Task HandleAsync(params SupplierOffersRefreshedEvent[] events)
	{
		await Scope.ServiceProvider.GetRequiredService<IDeliveryScheduleCacheRepository>()
			.InvalidateAsync(Supplier.Armtek, StorageCode, CancellationToken);

		await ActivatorUtilities.CreateInstance<TrackDeliveryDailyStatsHandler>(Scope.ServiceProvider)
			.Handle(new Batch<SupplierOffersRefreshedEvent>(events), CancellationToken);
	}

	private SupplierOffersRefreshedEvent Event(DateTime seenAt, params SupplierOfferSnapshot[] offers)
		=> new(Supplier.Armtek, StorageCode, seenAt, offers);

	private SupplierOfferSnapshot Offer(DateTime supplierDate, int probability, DateTime? guaranteedDate = null)
		=> new(ProductId, 1, 1000m, supplierDate, guaranteedDate, probability);

	private Task<List<DeliveryDailyStat>> GetStatsAsync()
		=> Context.Set<DeliveryDailyStat>().AsNoTracking().ToListAsync(CancellationToken);
}
