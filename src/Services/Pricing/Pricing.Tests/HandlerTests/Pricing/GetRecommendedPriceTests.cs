using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Handlers.Pricing;
using Pricing.Application.Handlers.Pricing.GetRecommendedPrice;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Entities.Offers;
using Pricing.Entities.Settings;
using Pricing.Enums;
using Pricing.Integration.Tests.DataBuilders.Offers;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests.HandlerTests.Pricing;

public sealed class GetRecommendedPriceTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private const int ProductId = 100;
	private const string StorageCode = "main";
	private const string Version = "current";

	[Theory]
	[InlineData(4, 100)]
	[InlineData(8, 125)]
	[InlineData(10, 140)]
	public async Task Quantity_ChangesCompositionAndWeightedUnitPrice(int quantity, decimal price)
	{
		var first = await AddOption(6, 100m, 2m);
		var second = await AddOption(4, 200m, 1m);

		var result = await GetRecommendation(quantity);

		result.UnitPrice.Should().Be(price);
		result.TotalPrice.Should().Be(price * quantity);
		result.MissingQuantity.Should().Be(0);
		result.Items.Sum(item => item.Quantity).Should().Be(quantity);
		result.Items[0].Option.PriceOfferId.Should().Be(first.PriceOfferId);
		result.Items[0].Quantity.Should().Be(Math.Min(quantity, 6));
		if (quantity > 6)
		{
			result.Items[1].Option.PriceOfferId.Should().Be(second.PriceOfferId);
			result.Items[1].Quantity.Should().Be(quantity - 6);
		}
		else
			result.Items.Should().ContainSingle();
	}

	[Fact]
	public async Task Shortage_ReturnsMissingQuantityWithoutFinalPrice()
	{
		await AddOption(2, 100m, 2m);
		await AddOption(3, 200m, 1m);

		var result = await GetRecommendation(7);

		result.MissingQuantity.Should().Be(2);
		result.UnitPrice.Should().BeNull();
		result.TotalPrice.Should().BeNull();
		result.Items.Sum(item => item.Quantity).Should().Be(5);
	}

	[Fact]
	public async Task NoOffers_ReturnsFullShortage()
	{
		var result = await GetRecommendation(3);

		result.MissingQuantity.Should().Be(3);
		result.UnitPrice.Should().BeNull();
		result.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task ExpiredEmptyAndClosedOffers_AreNotUsed()
	{
		await AddOption(100, 1m, 10m, configure: builder => builder.WithExpiresAt(DateTime.UtcNow.AddMinutes(-1)));
		await AddOption(100, 1m, 10m, configure: builder => builder.WithOrderTill(DateTime.UtcNow.AddMinutes(-1)));
		await AddOption(0, 1m, 10m);
		await AddOption(-1, 1m, 10m);
		var available = await AddOption(2, 120m, 1m);

		var result = await GetRecommendation(2);

		result.UnitPrice.Should().Be(120m);
		result.Items.Should().ContainSingle(item => item.Option.PriceOfferId == available.PriceOfferId);
	}

	[Fact]
	public async Task OtherProductsStoragesAndSources_DoNotAffectRecommendation()
	{
		await AddOption(100, 1m, 10m, productId: ProductId + 1);
		await AddOption(100, 1m, 10m, storageCode: "other");
		await AddOption(100, 1m, 10m, source: PriceOfferSource.FavoriteParts);
		await AddOption(2, 150m, 1m);

		var result = await GetRecommendation(3, sources: [PriceOfferSource.Armtek]);

		result.MissingQuantity.Should().Be(1);
		result.Items.Should().ContainSingle(item => item.Quantity == 2 && item.Option.Price == 150m);
	}

	[Fact]
	public async Task EqualScores_SelectCheaperOfferFirst()
	{
		await AddOption(3, 200m, 1m);
		var cheaper = await AddOption(3, 100m, 1m);

		var result = await GetRecommendation(2);

		result.UnitPrice.Should().Be(100m);
		result.Items.Should().ContainSingle(item => item.Option.PriceOfferId == cheaper.PriceOfferId);
	}

	[Fact]
	public async Task DifferentCurrencies_AreConvertedBeforeAveraging()
	{
		await AddOption(2, 10m, 2m, currencyId: 2);
		await AddOption(2, 30m, 1m);

		var result = await GetRecommendation(4);

		result.CurrencyId.Should().Be(1);
		result.UnitPrice.Should().Be(115m);
		result.TotalPrice.Should().Be(460m);
	}

	[Theory]
	[InlineData(0.01, 10.14)]
	[InlineData(0.05, 10.15)]
	[InlineData(0.1, 10.1)]
	[InlineData(1, 10)]
	[InlineData(5, 10)]
	public async Task WeightedPrice_UsesConfiguredRoundingStep(decimal step, decimal expectedPrice)
	{
		await AddOption(1, 10.03m, 2m);
		await AddOption(1, 10.26m, 1m);

		var result = await GetRecommendation(2, roundingStep: step);

		result.UnitPrice.Should().Be(expectedPrice);
		result.TotalPrice.Should().Be(expectedPrice * 2);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task NonPositiveQuantity_FailsValidation(int quantity)
	{
		var result = await new GetRecommendedPriceValidation().ValidateAsync(
			new GetRecommendedPriceQuery(ProductId, 1, StorageCode, quantity, []), CancellationToken);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(error => error.PropertyName == nameof(GetRecommendedPriceQuery.Quantity));
	}

	private async Task<PriceRecommendationDto> GetRecommendation(
		int quantity, IReadOnlyCollection<PriceOfferSource>? sources = null, decimal roundingStep = 0.01m)
	{
		var sender = new Mock<ISender>(MockBehavior.Strict);
		sender.Setup(x => x.Send(It.IsAny<RefreshOffersCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RefreshOffersResult([]));
		sender.Setup(x => x.Send(It.IsAny<CalculateCandidatesCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Unit.Value);
		var converter = new Mock<ICurrencyConverter>();
		converter.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
			.ReturnsAsync((decimal value, int currency, int _, CancellationToken _) => currency == 2 ? value * 20 : value);
		var markup = new Mock<IMarkupContainer>();
		markup.SetupGet(x => x.CurrentVersion).Returns(Version);
		var appliers = new Mock<IPriceApplierService>();
		appliers.Setup(x => x.GetCurrentConfigurationVersionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Version);
		var settings = new Mock<ISettingsService>();
		settings.Setup(x => x.GetOrDefault<PricingSetting>(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PricingSetting(new PricingSettingData { PriceRoundingStep = roundingStep }));
		var offers = new GetPriceOptionsForProductHandler(sender.Object,
			Scope.ServiceProvider.GetRequiredService<IReadRepository<ProductPriceOption, Guid>>(),
			converter.Object, markup.Object, appliers.Object, settings.Object);
		sender.Setup(x => x.Send(It.IsAny<GetPriceOptionsForProductQuery>(), It.IsAny<CancellationToken>()))
			.Returns((GetPriceOptionsForProductQuery query, CancellationToken token) => offers.Handle(query, token));
		var result = await new GetRecommendedPriceHandler(sender.Object, settings.Object).Handle(
			new GetRecommendedPriceQuery(ProductId, 1, StorageCode, quantity, sources ?? []), CancellationToken);
		return result.Item;
	}

	private async Task<ProductPriceOption> AddOption(
		int quantity, decimal price, decimal score,
		int productId = ProductId, string storageCode = StorageCode,
		int currencyId = 1, PriceOfferSource source = PriceOfferSource.Armtek,
		Action<PriceOfferDataBuilder>? configure = null)
	{
		var offerBuilder = new PriceOfferDataBuilder(Faker)
			.WithProductId(productId).WithStorageCode(storageCode).WithCurrencyId(currencyId)
			.WithAvailableQuantity(quantity).WithSource(source);
		configure?.Invoke(offerBuilder);
		var offer = await offerBuilder.BuildAndAddToDb(Context, CancellationToken);
		return await new ProductPriceOptionDataBuilder(Faker)
			.WithPriceOfferId(offer.Id).WithStorageCode(storageCode).WithCurrencyId(currencyId)
			.WithPrice(price).WithScore(score).WithVersions(Version, Version, Guid.Empty)
			.BuildAndAddToDb(Context, CancellationToken);
	}
}
