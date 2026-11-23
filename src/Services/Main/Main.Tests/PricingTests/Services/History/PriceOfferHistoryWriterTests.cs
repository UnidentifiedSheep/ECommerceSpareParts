using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Tests.PricingTests.DataBuilders.Offers;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.PricingTests.Services.History;

public class PriceOfferHistoryWriterTests : IntegrationTest
{
	public PriceOfferHistoryWriterTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private int ProductId => GetContext<ProductTestContext>().Products[0].Id;

	private int OtherProductId => GetContext<ProductTestContext>().Products[1].Id;

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	private int CurrencyId => GetContext<CurrencyTestContext>().Currencies[0].Id;
	private static readonly DateTime Day = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

	[Fact]
	public async Task TrackAsync_NewOffers_OpensHistoryWithDeliveryInDays()
	{
		await TrackAsync(Day, Offer(1500m, Day.AddDays(3).AddHours(5)), Offer(1480m, Day.AddDays(5)));

		var histories = await GetHistoriesAsync();

		histories.Should().HaveCount(2);
		histories.Should().OnlyContain(x => x.IsOpen && x.FirstSeenAt == Day && x.LastSeenAt == Day);
		histories.Single(x => x.PurchasePrice == 1500m).DeliveryTime.Should().Be(TimeSpan.FromDays(3));
		histories.Single(x => x.PurchasePrice == 1480m).DeliveryTime.Should().Be(TimeSpan.FromDays(5));
	}

	[Fact]
	public async Task TrackAsync_SameOffer_ExtendsLastSeenAt()
	{
		await TrackAsync(Day, Offer(1500m, Day.AddDays(3)));
		await TrackAsync(Day.AddHours(6), Offer(1500m, Day.AddDays(3)));

		var history = (await GetHistoriesAsync()).Should().ContainSingle().Subject;

		history.IsOpen.Should().BeTrue();
		history.FirstSeenAt.Should().Be(Day);
		history.LastSeenAt.Should().Be(Day.AddHours(6));
	}

	[Fact]
	public async Task TrackAsync_DuplicateOffers_StoredOnce()
	{
		await TrackAsync(Day, Offer(1500m, Day.AddDays(3)), Offer(1500m, Day.AddDays(3)));

		(await GetHistoriesAsync()).Should().ContainSingle();
	}

	[Fact]
	public async Task TrackAsync_PriceChanged_ClosesOldAndOpensNew()
	{
		await TrackAsync(Day, Offer(1500m, Day.AddDays(3)));
		await TrackAsync(Day.AddDays(1), Offer(1650m, Day.AddDays(4)));

		var histories = await GetHistoriesAsync();

		var old = histories.Single(x => x.PurchasePrice == 1500m);
		old.IsOpen.Should().BeFalse();
		old.LastSeenAt.Should().Be(Day);
		var current = histories.Single(x => x.PurchasePrice == 1650m);
		current.IsOpen.Should().BeTrue();
		current.FirstSeenAt.Should().Be(Day.AddDays(1));
	}

	[Fact]
	public async Task TrackAsync_OfferDisappearedAndReturned_StartsNewPeriod()
	{
		await TrackAsync(Day, Offer(1500m, Day.AddDays(3)));
		await TrackAsync(Day.AddDays(1));
		await TrackAsync(Day.AddDays(2), Offer(1500m, Day.AddDays(5)));

		var histories = await GetHistoriesAsync();

		histories.Should().HaveCount(2);
		histories.Single(x => !x.IsOpen).LastSeenAt.Should().Be(Day);
		histories.Single(x => x.IsOpen).FirstSeenAt.Should().Be(Day.AddDays(2));
	}

	[Fact]
	public async Task TrackAsync_OtherSourceAndProduct_AreNotTouched()
	{
		await TrackAsync(Day, Offer(1500m, Day.AddDays(3)));
		var otherSource = new PriceOfferDataBuilder(Faker)
			.WithProductId(ProductId)
			.WithStorageCode(StorageCode)
			.WithCurrencyId(CurrencyId)
			.WithSource(PriceOfferSource.Tmtr)
			.WithPrice(900m)
			.WithDeliveryDate(Day.AddDays(2))
			.Build();
		await GetWriter().TrackAsync(
			StorageCode,
			PriceOfferSource.Tmtr,
			[ProductId],
			[otherSource],
			Day,
			CancellationToken);

		await TrackAsync(Day.AddDays(1), [OtherProductId], []);

		var histories = await GetHistoriesAsync();

		histories.Should().HaveCount(2);
		histories.Should().OnlyContain(x => x.IsOpen);
	}

	private Task TrackAsync(DateTime seenAt, params PriceOffer[] offers)
		=> TrackAsync(seenAt, [ProductId], offers);

	private Task TrackAsync(DateTime seenAt, int[] productIds, PriceOffer[] offers)
		=> GetWriter().TrackAsync(
			StorageCode,
			PriceOfferSource.Armtek,
			productIds,
			offers,
			seenAt,
			CancellationToken);

	private IPriceOfferHistoryWriter GetWriter()
		=> Scope.ServiceProvider.GetRequiredService<IPriceOfferHistoryWriter>();

	private PriceOffer Offer(decimal price, DateTime deliveryDate)
		=> new PriceOfferDataBuilder(Faker)
			.WithProductId(ProductId)
			.WithStorageCode(StorageCode)
			.WithCurrencyId(CurrencyId)
			.WithSource(PriceOfferSource.Armtek)
			.WithPrice(price)
			.WithDeliveryDate(deliveryDate)
			.Build();

	private Task<List<PriceOfferHistory>> GetHistoriesAsync()
		=> Context.Set<PriceOfferHistory>().AsNoTracking().ToListAsync(CancellationToken);
}
