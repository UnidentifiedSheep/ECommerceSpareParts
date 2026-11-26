using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Bogus;
using Enums;
using FluentAssertions;
using Integrations.Supplier.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Pricing.Abstractions.Events;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;
using Pricing.Application.Services;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Tests.PricingTests.DataBuilders.Offers;

namespace Tests.PricingTests.Services;

public sealed class OfferRefreshServicePublishTests
{
	private const string StorageCode = "main";
	private static readonly DateTime SeenAt = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

	[Fact]
	public async Task RefreshOffers_PublishesOnlySupplierOffersOfRequestedStorage()
	{
		var faker = new Faker();
		var supplierOffer = Offer(faker, 1, PriceOfferSource.Armtek, StorageCode);
		var otherStorageOffer = Offer(faker, 1, PriceOfferSource.Armtek, "other");
		var warehouseOffer = Offer(faker, 1, PriceOfferSource.OurWarehouse, StorageCode);

		var converter = new Mock<ISupplierOfferConverterService>();
		converter
			.Setup(x => x.ConvertAsync(
				1,
				StorageCode,
				It.IsAny<IReadOnlyDictionary<Supplier, IReadOnlyList<SupplierPosition>>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(
			[
				new SupplierOfferConversionResult
				{
					Supplier = Supplier.Armtek,
					Offers = [supplierOffer, otherStorageOffer, warehouseOffer],
					NotFoundCurrencies = []
				}
			]);
		var states = new Mock<IPriceOfferRefreshStateRepository>();
		states
			.Setup(x => x.UpsertStatesAsync(
				It.IsAny<IReadOnlyCollection<PriceOfferRefreshState>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((IReadOnlyCollection<PriceOfferRefreshState> items, CancellationToken _) =>
				items.Where(x => x.ProductId == 1).Select(x => x.GetId()).ToHashSet());
		var eventScope = new DomainEventScope();

		var service = new OfferRefreshService(
			converter.Object,
			Mock.Of<IPriceOfferRepository>(),
			Mock.Of<IIntegrationEventScope>(),
			states.Object,
			Mock.Of<ISupplierOfferRequestMarkerService>(),
			eventScope,
			NullLogger<OfferRefreshService>.Instance);

		await service.RefreshOffersAsync(
			SeenAt,
			StorageCode,
			Supplier.Armtek,
			new Dictionary<int, IReadOnlyList<SupplierPosition>>
			{
				[1] = [],
				[2] = []
			});

		var published = eventScope.Flush()
			.OfType<Batch<SupplierOffersRefreshedEvent>>()
			.SelectMany(x => x.Items)
			.Should().ContainSingle().Subject;
		published.Supplier.Should().Be(Supplier.Armtek);
		published.StorageCode.Should().Be(StorageCode);
		published.SeenAt.Should().Be(SeenAt);
		published.Offers.Should().ContainSingle()
			.Which.PurchasePrice.Should().Be(supplierOffer.PurchasePrice);
	}

	[Fact]
	public async Task RefreshOffers_NothingRefreshed_PublishesNothing()
	{
		var states = new Mock<IPriceOfferRefreshStateRepository>();
		states
			.Setup(x => x.UpsertStatesAsync(
				It.IsAny<IReadOnlyCollection<PriceOfferRefreshState>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(new HashSet<PriceOfferRefreshStateKey>());
		var eventScope = new DomainEventScope();

		var service = new OfferRefreshService(
			Mock.Of<ISupplierOfferConverterService>(),
			Mock.Of<IPriceOfferRepository>(),
			Mock.Of<IIntegrationEventScope>(),
			states.Object,
			Mock.Of<ISupplierOfferRequestMarkerService>(),
			eventScope,
			NullLogger<OfferRefreshService>.Instance);

		await service.RefreshOffersAsync(
			SeenAt,
			StorageCode,
			Supplier.Armtek,
			new Dictionary<int, IReadOnlyList<SupplierPosition>> { [1] = [] });

		eventScope.Flush().Should().BeEmpty();
	}

	private static PriceOffer Offer(Faker faker, int productId, PriceOfferSource source, string storageCode)
		=> new PriceOfferDataBuilder(faker)
			.WithProductId(productId)
			.WithSource(source)
			.WithStorageCode(storageCode)
			.Build();
}
