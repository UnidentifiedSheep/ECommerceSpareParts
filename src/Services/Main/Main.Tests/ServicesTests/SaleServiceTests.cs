using FluentAssertions;
using Main.Application.Interfaces.Persistence;
using Main.Application.Dtos.Sale;
using Main.Application.Models.Storage;
using Main.Application.Services;
using MediatR;
using Moq;

namespace Tests.ServicesTests;

public class SaleServiceTests
{
    private readonly SaleService _service = new(
        Mock.Of<ISender>(),
        Mock.Of<IProductRepository>());

    [Fact]
    public void DistributeDetails_WithNewSaleContent_DistributesFromLargestLotsFirst()
    {
        var purchaseDate = new DateTime(2026, 1, 1);
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 2, buyPrice: 100m, purchaseDate),
            Lot(2, productId: 10, count: 5, buyPrice: 80m, purchaseDate),
            Lot(3, productId: 10, count: 5, buyPrice: 120m, purchaseDate)
        };
        var saleContents = new[]
        {
            NewSaleContent(productId: 10, count: 7, price: 200m, priceWithDiscount: 180m)
        };

        var result = _service.DistributeDetails(storageLots, saleContents);

        result.Should().ContainSingle();
        var saleContent = result.Single();
        saleContent.ProductId.Should().Be(10);
        saleContent.Count.Should().Be(7);
        saleContent.Price.Should().Be(180m);
        saleContent.TotalSum.Should().Be(1260m);
        saleContent.Discount.Should().Be(0.1m);

        saleContent.Details.Should().HaveCount(2);
        saleContent.Details[0].StorageContentId.Should().Be(3);
        saleContent.Details[0].Count.Should().Be(5);
        saleContent.Details[0].BuyPrice.Should().Be(120m);
        saleContent.Details[1].StorageContentId.Should().Be(2);
        saleContent.Details[1].Count.Should().Be(2);
        saleContent.Details[1].BuyPrice.Should().Be(80m);
    }

    [Fact]
    public void DistributeDetails_WithEditSaleContent_DistributesDetails()
    {
        var purchaseDate = new DateTime(2026, 1, 1);
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 3, buyPrice: 100m, purchaseDate)
        };
        var saleContents = new[]
        {
            new EditSaleContentDto
            {
                ProductId = 10,
                Count = 3,
                Price = 200m,
                PriceWithDiscount = 150m
            }
        };

        var result = _service.DistributeDetails(storageLots, saleContents);

        result.Should().ContainSingle();
        var saleContent = result.Single();
        saleContent.ProductId.Should().Be(10);
        saleContent.Count.Should().Be(3);
        saleContent.Price.Should().Be(150m);
        saleContent.TotalSum.Should().Be(450m);
        saleContent.Details.Should().ContainSingle();
        saleContent.Details.Single().StorageContentId.Should().Be(1);
        saleContent.Details.Single().Count.Should().Be(3);
    }

    [Fact]
    public void DistributeDetails_WithMultipleSaleContents_DoesNotReuseTakenQuantity()
    {
        var purchaseDate = new DateTime(2026, 1, 1);
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 5, buyPrice: 100m, purchaseDate),
            Lot(2, productId: 10, count: 5, buyPrice: 90m, purchaseDate)
        };
        var saleContents = new[]
        {
            NewSaleContent(productId: 10, count: 4),
            NewSaleContent(productId: 10, count: 4)
        };

        var result = _service.DistributeDetails(storageLots, saleContents);

        result.Should().HaveCount(2);
        result[0].Details.Should().ContainSingle();
        result[0].Details.Single().StorageContentId.Should().Be(1);
        result[0].Details.Single().Count.Should().Be(4);
        result[1].Details.Should().HaveCount(2);
        result[1].Details[0].StorageContentId.Should().Be(1);
        result[1].Details[0].Count.Should().Be(1);
        result[1].Details[1].StorageContentId.Should().Be(2);
        result[1].Details[1].Count.Should().Be(3);
    }

    [Fact]
    public void DistributeDetails_WithMultipleSaleContents_DoesNotReuseFullyConsumedLot()
    {
        var purchaseDate = new DateTime(2026, 1, 1);
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 1, buyPrice: 100m, purchaseDate),
            Lot(2, productId: 10, count: 1, buyPrice: 90m, purchaseDate)
        };
        var saleContents = new[]
        {
            NewSaleContent(productId: 10, count: 1),
            NewSaleContent(productId: 10, count: 1)
        };

        var result = _service.DistributeDetails(storageLots, saleContents);

        result.Should().HaveCount(2);
        result[0].Details.Should().ContainSingle();
        result[0].Details.Single().StorageContentId.Should().Be(1);
        result[0].Details.Single().Count.Should().Be(1);
        result[1].Details.Should().ContainSingle();
        result[1].Details.Single().StorageContentId.Should().Be(2);
        result[1].Details.Single().Count.Should().Be(1);
        result.SelectMany(x => x.Details)
            .GroupBy(x => x.StorageContentId)
            .Should()
            .OnlyContain(x => x.Sum(z => z.Count) <= storageLots.Single(lot => lot.Id == x.Key).Count);
    }

    [Fact]
    public void DistributeDetails_WithMixedFullAndPartialConsumption_DistributesAllRequestedCounts()
    {
        var purchaseDate = new DateTime(2026, 1, 1);
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 2, buyPrice: 100m, purchaseDate),
            Lot(2, productId: 10, count: 2, buyPrice: 90m, purchaseDate),
            Lot(3, productId: 10, count: 1, buyPrice: 80m, purchaseDate)
        };
        var saleContents = new[]
        {
            NewSaleContent(productId: 10, count: 3),
            NewSaleContent(productId: 10, count: 2)
        };

        var result = _service.DistributeDetails(storageLots, saleContents);

        result.Should().HaveCount(2);
        result.Sum(x => x.Count).Should().Be(5);
        result.SelectMany(x => x.Details).Sum(x => x.Count).Should().Be(5);
        result.SelectMany(x => x.Details)
            .GroupBy(x => x.StorageContentId)
            .Should()
            .OnlyContain(x => x.Sum(z => z.Count) == storageLots.Single(lot => lot.Id == x.Key).Count);
    }

    [Fact]
    public void DistributeDetails_WithMultipleProducts_DoesNotShareStorageBetweenProducts()
    {
        var purchaseDate = new DateTime(2026, 1, 1);
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 1, buyPrice: 100m, purchaseDate),
            Lot(2, productId: 20, count: 1, buyPrice: 90m, purchaseDate)
        };
        var saleContents = new[]
        {
            NewSaleContent(productId: 10, count: 1),
            NewSaleContent(productId: 20, count: 1)
        };

        var result = _service.DistributeDetails(storageLots, saleContents);

        result.Should().HaveCount(2);
        result.Single(x => x.ProductId == 10).Details.Should().ContainSingle(x => x.StorageContentId == 1);
        result.Single(x => x.ProductId == 20).Details.Should().ContainSingle(x => x.StorageContentId == 2);
    }

    [Fact]
    public void DistributeDetails_WhenStorageLotCountIsInvalid_Throws()
    {
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 0)
        };
        var saleContents = new[]
        {
            NewSaleContent(productId: 10, count: 1)
        };

        var act = () => _service.DistributeDetails(storageLots, saleContents);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid taken quantity");
    }

    [Fact]
    public void DistributeDetails_WhenStorageForProductIsMissing_Throws()
    {
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 1)
        };
        var saleContents = new[]
        {
            NewSaleContent(productId: 20, count: 1)
        };

        var act = () => _service.DistributeDetails(storageLots, saleContents);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No storage for product 20");
    }

    [Fact]
    public void DistributeDetails_WhenStorageQuantityIsNotEnough_Throws()
    {
        var storageLots = new[]
        {
            Lot(1, productId: 10, count: 2)
        };
        var saleContents = new[]
        {
            NewSaleContent(productId: 10, count: 3)
        };

        var act = () => _service.DistributeDetails(storageLots, saleContents);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Unable to distribute details");
    }

    private static StorageLot Lot(
        int id,
        int productId,
        int count,
        decimal buyPrice = 100m,
        DateTime? purchaseDate = null)
    {
        return new StorageLot(
            id,
            productId,
            1,
            buyPrice,
            count,
            purchaseDate ?? new DateTime(2026, 1, 1));
    }

    private static NewSaleContentDto NewSaleContent(
        int productId,
        int count,
        decimal price = 200m,
        decimal priceWithDiscount = 180m)
    {
        return new NewSaleContentDto
        {
            ProductId = productId,
            Count = count,
            Price = price,
            PriceWithDiscount = priceWithDiscount
        };
    }
}
