using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Enums;
using FluentAssertions;
using Main.Entities.Logistics.Enums;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Logistics.Abstractions.Interfaces;
using Main.Modules.Logistics.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pricing.Application.Commands.Pricing;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Queries.Pricing;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Pricing.Entities.Settings;
using Tests.DataBuilders.Logistics;
using Tests.Extensions;
using Tests.PricingTests.DataBuilders.Offers;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.PricingTests.HandlerTests.Pricing;

public sealed class GetPriceOptionsDeliveryDatesTests : IntegrationTest
{
	private const string Version = "current";

	public GetPriceOptionsDeliveryDatesTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private int ProductId => GetContext<ProductTestContext>().Products[0].Id;

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	private int CurrencyId => GetContext<CurrencyTestContext>().Currencies[0].Id;

	[Fact]
	public async Task Handle_SupplierOffer_ReturnsArrivalCountedFromNowBySchedule()
	{
		await new DeliveryScheduleBuilder(Faker)
			.WithSupplier(Supplier.Armtek)
			.WithStorageCode(StorageCode)
			.WithDays(DeliveryDays.All)
			.WithArrivalTime(new TimeOnly(14, 0))
			.BuildAndAddToDb(Context, CancellationToken);
		await Scope.ServiceProvider.GetRequiredService<IDeliveryScheduleCacheRepository>()
			.InvalidateAsync(Supplier.Armtek, StorageCode, CancellationToken);
		await AddOptionAsync(PriceOfferSource.Armtek, deliveryDays: 2);
		var today = DateOnly.FromDateTime(DateTime.UtcNow);

		var result = await HandleAsync();

		var offer = result.PriceOptions.Should().ContainSingle().Subject.PriceOffer;
		offer.DeliveryDate.Should().Be(today.AddDays(2).ToDateTime(new TimeOnly(14, 0), DateTimeKind.Utc));
		offer.GuaranteedDeliveryDate.Should().Be(today.AddDays(3).ToDateTime(new TimeOnly(14, 0), DateTimeKind.Utc));
	}

	[Fact]
	public async Task Handle_OurWarehouseOffer_ReturnsNoDates()
	{
		await AddOptionAsync(PriceOfferSource.OurWarehouse, deliveryDays: 2);

		var result = await HandleAsync();

		var offer = result.PriceOptions.Should().ContainSingle().Subject.PriceOffer;
		offer.DeliveryDate.Should().BeNull();
		offer.GuaranteedDeliveryDate.Should().BeNull();
	}

	private async Task<GetPriceOptionsForProductResult> HandleAsync()
	{
		var sender = new Mock<ISender>(MockBehavior.Strict);
		sender.Setup(x => x.Send(It.IsAny<RefreshOffersCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RefreshOffersResult([]));
		sender.Setup(x => x.Send(It.IsAny<CalculateCandidatesCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Unit.Value);
		var converter = new Mock<ICurrencyConverter>();
		converter
			.Setup(x => x.ConvertAsync(
				It.IsAny<decimal>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((decimal value, int _, int _, CancellationToken _) => value);
		var markup = new Mock<IMarkupContainer>();
		markup.SetupGet(x => x.CurrentVersion).Returns(Version);
		var appliers = new Mock<IPriceApplierService>();
		appliers.Setup(x => x.GetCurrentConfigurationVersionAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(Version);
		var settings = new Mock<ISettingsService>();
		settings.Setup(x => x.GetOrDefault<PricingSetting>(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PricingSetting(new PricingSettingData()));

		return await new GetPriceOptionsForProductHandler(
				sender.Object,
				Scope.ServiceProvider.GetRequiredService<IReadRepository<ProductPriceOption, Guid>>(),
				converter.Object,
				markup.Object,
				appliers.Object,
				settings.Object,
				Scope.ServiceProvider.GetRequiredService<IDeliveryEstimator>())
			.Handle(
				new GetPriceOptionsForProductQuery(ProductId, CurrencyId, StorageCode, [], null, []),
				CancellationToken);
	}

	private async Task AddOptionAsync(PriceOfferSource source, int deliveryDays)
	{
		var offer = await new PriceOfferDataBuilder(Faker)
			.WithProductId(ProductId)
			.WithStorageCode(StorageCode)
			.WithCurrencyId(CurrencyId)
			.WithSource(source)
			.WithDeliveryDays(deliveryDays)
			.BuildAndAddToDb(Context, CancellationToken);
		await new ProductPriceOptionDataBuilder(Faker)
			.WithPriceOfferId(offer.Id)
			.WithStorageCode(StorageCode)
			.WithCurrencyId(CurrencyId)
			.WithVersions(Version, Version, Guid.Empty)
			.BuildAndAddToDb(Context, CancellationToken);
	}
}
