using Enums;
using FluentAssertions;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Pricing.Entities.Offers;
using Pricing.Enums;
using Tests.DataBuilders.Storage;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.PricingTests.HandlerTests;

public sealed class StorageContentChangedTests : IntegrationTest
{
	public StorageContentChangedTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	[Fact]
	public async Task AddingStorageContent_CreatesOurWarehouseOffers()
	{
		var storage = GetContext<StorageTestContext>().Storages.First();
		var contents = NewContents(storage.Code, 3);

		await Mediator.Send(
			new AddContentCommand(contents, storage.Code, StorageMovementType.StorageContentAddition),
			CancellationToken);

		var storageContents = await Context.Set<StorageContent>()
			.AsNoTracking()
			.Where(x => x.StorageCode == storage.Code)
			.ToListAsync(CancellationToken);
		var offers = await Context.Set<PriceOffer>()
			.AsNoTracking()
			.Where(x => x.Source == PriceOfferSource.OurWarehouse && x.OfferForStorage == storage.Code)
			.ToListAsync(CancellationToken);

		storageContents.Should().NotBeEmpty();
		offers.Select(x => x.SourceKey)
			.Should()
			.BeEquivalentTo(storageContents.Select(x => x.Id.ToString()));
		foreach (var content in storageContents)
		{
			var offer = offers.Single(x => x.SourceKey == content.Id.ToString());
			offer.ProductId.Should().Be(content.ProductId);
			offer.AvailableQuantity.Should().Be(content.Count);
			offer.PurchasePrice.Should().Be(content.BuyPrice);
			offer.CurrencyId.Should().Be(content.CurrencyId);
		}
	}

	private List<NewStorageContentDto> NewContents(string storageCode, int count)
		=> new StorageContentBuilder(Faker)
			.WithProducts(GetContext<ProductTestContext>().Products)
			.WithCurrencyId(GetContext<CurrencyTestContext>().Currencies[0].Id)
			.WithPurchaseDate(DateTime.UtcNow)
			.WithStorageCode(storageCode)
			.BuildMany(count)
			.Select(x => new NewStorageContentDto
			{
				BuyPrice = x.BuyPrice,
				Count = x.Count,
				CurrencyId = x.CurrencyId,
				ProductId = x.ProductId,
				PurchaseDate = x.PurchaseDatetime
			})
			.ToList();
}
