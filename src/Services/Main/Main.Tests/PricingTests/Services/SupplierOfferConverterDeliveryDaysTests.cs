using Enums;
using FluentAssertions;
using Integrations.Supplier.Models;
using Main.Entities.Logistics.Enums;
using Main.Modules.Logistics.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Interfaces.Pricing;
using Tests.DataBuilders.Logistics;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.PricingTests.Services;

public sealed class SupplierOfferConverterDeliveryDaysTests : IntegrationTest
{
	public SupplierOfferConverterDeliveryDaysTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private int ProductId => GetContext<ProductTestContext>().Products[0].Id;

	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;

	private string CurrencyCode => GetContext<CurrencyTestContext>().Currencies[0].Code;

	[Fact]
	public async Task Convert_StoresReceivingDaysInsteadOfDates()
	{
		await new DeliveryScheduleBuilder(Faker)
			.WithSupplier(Supplier.Armtek)
			.WithStorageCode(StorageCode)
			.WithDays(DeliveryDays.All)
			.BuildAndAddToDb(Context, CancellationToken);
		await Scope.ServiceProvider.GetRequiredService<IDeliveryScheduleCacheRepository>()
			.InvalidateAsync(Supplier.Armtek, StorageCode, CancellationToken);
		var now = DateTime.UtcNow;

		var result = await Scope.ServiceProvider.GetRequiredService<ISupplierOfferConverterService>().ConvertAsync(
			ProductId,
			StorageCode,
			Supplier.Armtek,
			[Position("fast", now.AddDays(3), now.AddDays(5)), Position("past", now.AddDays(-2), now)],
			CancellationToken);

		var offers = result.Offers.ToDictionary(x => x.SourceKey);
		offers["fast"].DeliveryDays.Should().Be(3);
		offers["fast"].GuaranteedDeliveryDays.Should().Be(5);
		offers["past"].DeliveryDays.Should().Be(0);
		offers["past"].GuaranteedDeliveryDays.Should().Be(0);
	}

	private SupplierPosition Position(string id, DateTime deliveryDate, DateTime guaranteedDate)
		=> new()
		{
			Id = id,
			PurchaseInfo = new PurchaseInfo
			{
				AvailableQuantity = 5,
				MinimumPurchaseQuantity = 1,
				QuantityCoefficient = 1,
				PriceInfo = new PriceInfo { Price = 100m, CurrencyCode = CurrencyCode }
			},
			DeliveryInfo = new DeliveryInfo
			{
				DeliveryDate = deliveryDate,
				GuaranteedDeliveryDate = guaranteedDate,
				DeliveryProbability = 90,
				OrderTill = deliveryDate
			}
		};
}
