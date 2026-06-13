using FluentAssertions;
using Main.Abstractions.Constants;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Sales.CreateSale;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.Storage;
using Tests.TestContexts;
using Tests.TestContexts.Storage;


namespace Tests.HandlersTests.Sales;

public class CreateSaleTests : IntegrationTest
{
    public CreateSaleTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageContentTestContext>();
        RegisterBasicContext<UsersTestContext>();
    }

    [Fact]
    public async Task CreateSale_CreatesSaleAndSubtractsStorageContent()
    {
        var buyer = Buyer();
        var storageContent = StorageContent();
        var product = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == storageContent.ProductId);
        var originalContents = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == storageContent.ProductId && x.StorageName == storageContent.StorageName)
            .ToDictionaryAsync(x => x.Id);
        var originalStock = product.Stock.Value;
        var command = CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [NewContent(storageContent.ProductId, count: 1, price: 100m, priceWithDiscount: 80m)]);

        var result = await Mediator.Send(command);

        result.Sale.Id.Should().NotBeEmpty();
        result.Sale.Buyer.Id.Should().Be(buyer.Id);
        result.Sale.Currency.Id.Should().Be(storageContent.CurrencyId);
        result.Sale.Storage.Should().Be(storageContent.StorageName);
        result.Sale.Comment.Should().Be(command.Comment);
        result.Sale.TotalSum.Should().Be(80m);

        var sale = await Context.Sales
            .Include(x => x.Contents)
            .ThenInclude(x => x.Details)
            .AsNoTracking()
            .SingleAsync(x => x.Id == result.Sale.Id);
        sale.BuyerId.Should().Be(buyer.Id);
        sale.CurrencyId.Should().Be(storageContent.CurrencyId);
        sale.StorageName.Should().Be(storageContent.StorageName);

        var saleContent = sale.Contents.Should().ContainSingle().Subject;
        saleContent.ProductId.Should().Be(storageContent.ProductId);
        saleContent.Count.Should().Be(1);
        saleContent.Price.Should().Be(80m);
        saleContent.TotalSum.Should().Be(80m);
        saleContent.Discount.Should().Be(0.2m);

        var detail = saleContent.Details.Should().ContainSingle().Subject;
        originalContents.Should().ContainKey(detail.StorageContentId);
        var affectedStorageContent = originalContents[detail.StorageContentId];
        detail.CurrencyId.Should().Be(affectedStorageContent.CurrencyId);
        detail.BuyPrice.Should().Be(affectedStorageContent.BuyPrice);
        detail.Count.Should().Be(1);
        detail.PurchaseDatetime.Should().BeCloseTo(
            affectedStorageContent.PurchaseDatetime.ToUniversalTime(),
            TimeSpan.FromMilliseconds(1));

        var updatedContent = await Context.StorageContents
            .AsNoTracking()
            .SingleAsync(x => x.Id == detail.StorageContentId);
        updatedContent.Count.Should().Be(affectedStorageContent.Count - 1);

        var updatedProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == storageContent.ProductId);
        updatedProduct.Stock.Value.Should().Be(originalStock - 1);

        var saleTransaction = await Context.Transactions.AsNoTracking().SingleAsync(x => x.Id == sale.TransactionId);
        saleTransaction.SenderId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
        saleTransaction.ReceiverId.Should().Be(buyer.Id);
        saleTransaction.Amount.Should().Be(80m);
        saleTransaction.CurrencyId.Should().Be(storageContent.CurrencyId);
        saleTransaction.SourceType.Should().Be(TransactionSourceType.Sale);

        var movement = await Context.Events.OfType<StorageMovementEvent>().AsNoTracking().SingleAsync();
        movement.Data.ProductId.Should().Be(storageContent.ProductId);
        movement.Data.StorageName.Should().Be(storageContent.StorageName);
        movement.Data.Count.Should().Be(1);
        movement.Data.MovementType.Should().Be(StorageMovementType.Sale);
    }

    [Fact]
    public async Task CreateSale_WithPayedSum_CreatesPaymentTransaction()
    {
        var buyer = Buyer();
        var storageContent = StorageContent();
        var payedSum = 50m;
        var command = CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [NewContent(storageContent.ProductId, count: 1)],
            payedSum);

        await Mediator.Send(command);

        var transactions = await Context.Transactions.AsNoTracking().ToListAsync();
        transactions.Should().HaveCount(2);

        var payment = transactions.Single(x => x.SourceType == TransactionSourceType.Manual);
        payment.SenderId.Should().Be(buyer.Id);
        payment.ReceiverId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
        payment.Amount.Should().Be(payedSum);
        payment.CurrencyId.Should().Be(storageContent.CurrencyId);
    }

    [Fact]
    public async Task CreateSale_WithZeroPayedSum_DoesNotCreatePaymentTransaction()
    {
        var buyer = Buyer();
        var storageContent = StorageContent();
        var command = CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [NewContent(storageContent.ProductId, count: 1)],
            0m);

        await Mediator.Send(command);

        var transactions = await Context.Transactions.AsNoTracking().ToListAsync();
        transactions.Should().ContainSingle();
        transactions.Single().SourceType.Should().Be(TransactionSourceType.Sale);
    }

    [Fact]
    public async Task CreateSale_WithBuyerReservation_SubtractsReservationCount()
    {
        var buyer = Buyer();
        var storageContent = StorageContent();
        var reservation = await new StorageContentReservationBuilder(Faker)
            .WithUserId(buyer.Id)
            .WithProductId(storageContent.ProductId)
            .WithReservedCount(2)
            .BuildAndAddToDb(Context);

        var command = CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [NewContent(storageContent.ProductId, count: 2)]);

        await Mediator.Send(command);

        var updatedReservation = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);
        updatedReservation.CurrentCount.Should().Be(2);
        updatedReservation.IsDone.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSale_WithDuplicateProductContents_AggregatesStockCheckAndCreatesSeparateContents()
    {
        var buyer = Buyer();
        var storageContent = StorageContentWithCountAtLeast(2);
        var countOnStorage = await CountOnStorage(storageContent);
        var command = CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [
                NewContent(storageContent.ProductId, count: 1, price: 100m, priceWithDiscount: 90m),
                NewContent(storageContent.ProductId, count: 1, price: 200m, priceWithDiscount: 180m)
            ]);

        var result = await Mediator.Send(command);

        result.Sale.TotalSum.Should().Be(270m);

        var sale = await Context.Sales
            .Include(x => x.Contents)
            .ThenInclude(x => x.Details)
            .AsNoTracking()
            .SingleAsync(x => x.Id == result.Sale.Id);
        sale.Contents.Should().HaveCount(2);
        sale.Contents.Sum(x => x.Count).Should().Be(2);
        sale.Contents.SelectMany(x => x.Details).Sum(x => x.Count).Should().Be(2);

        var updatedCountOnStorage = await CountOnStorage(storageContent);
        updatedCountOnStorage.Should().Be(countOnStorage - 2);
    }

    [Fact]
    public async Task CreateSale_WhenStorageCountIsNotEnough_ThrowsNotEnoughCountOnStorageException()
    {
        var buyer = Buyer();
        var storageContent = StorageContent();
        var countOnStorage = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == storageContent.ProductId && x.StorageName == storageContent.StorageName)
            .SumAsync(x => x.Count);
        var command = CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [NewContent(storageContent.ProductId, countOnStorage + 1)]);

        await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Mediator.Send(command));

        var salesCount = await Context.Sales.AsNoTracking().CountAsync();
        salesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateSale_WithOtherBuyerReservationAndNoConfirmation_ThrowsSaleSoftConfirmationNeededException()
    {
        var buyer = Buyer();
        var otherBuyer = GetContext<UsersTestContext>().Users.First(x => x.Id != buyer.Id);
        var storageContent = StorageContent();
        var countOnStorage = await CountOnStorage(storageContent);
        await new StorageContentReservationBuilder(Faker)
            .WithUserId(otherBuyer.Id)
            .WithProductId(storageContent.ProductId)
            .WithReservedCount(countOnStorage + 1)
            .WithCurrentCount(countOnStorage)
            .BuildAndAddToDb(Context);

        var command = CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [NewContent(storageContent.ProductId, count: 1)]);

        await Assert.ThrowsAsync<SaleSoftConfirmationNeededException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreateSale_WithOtherBuyerReservationAndValidConfirmation_CreatesSale()
    {
        var buyer = Buyer();
        var otherBuyer = GetContext<UsersTestContext>().Users.First(x => x.Id != buyer.Id);
        var storageContent = StorageContent();
        var countOnStorage = await CountOnStorage(storageContent);
        await new StorageContentReservationBuilder(Faker)
            .WithUserId(otherBuyer.Id)
            .WithProductId(storageContent.ProductId)
            .WithReservedCount(countOnStorage + 1)
            .WithCurrentCount(countOnStorage)
            .BuildAndAddToDb(Context);
        var commandWithoutConfirmation = CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [NewContent(storageContent.ProductId, count: 1)]);

        var exception = await Assert.ThrowsAsync<SaleSoftConfirmationNeededException>(() =>
            Mediator.Send(commandWithoutConfirmation));
        var confirmationCode = exception.Arguments.Should().ContainSingle().Subject.Should().BeOfType<string>().Subject;
        var command = commandWithoutConfirmation with { ConfirmationCode = confirmationCode };

        var result = await Mediator.Send(command);

        result.Sale.Id.Should().NotBeEmpty();
        var salesCount = await Context.Sales.AsNoTracking().CountAsync();
        salesCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateSale_WithEmptyContents_ThrowsValidationException()
    {
        var storageContent = StorageContent();
        var command = CreateValidCommand() with
        {
            CurrencyId = storageContent.CurrencyId,
            StorageName = storageContent.StorageName,
            Contents = []
        };

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSale_WithInvalidContentCount_ThrowsValidationException(int count)
    {
        var command = CreateValidCommand();
        var content = command.Contents.Single() with { Count = count };

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(command with { Contents = [content] }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.001)]
    public async Task CreateSale_WithInvalidContentPrice_ThrowsValidationException(decimal price)
    {
        var command = CreateValidCommand();
        var content = command.Contents.Single() with { Price = price };

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(command with { Contents = [content] }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.001)]
    public async Task CreateSale_WithInvalidPriceWithDiscount_ThrowsValidationException(decimal priceWithDiscount)
    {
        var command = CreateValidCommand();
        var content = command.Contents.Single() with { PriceWithDiscount = priceWithDiscount };

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(command with { Contents = [content] }));
    }

    [Fact]
    public async Task CreateSale_WithPriceWithDiscountGreaterThanPrice_ThrowsValidationException()
    {
        var command = CreateValidCommand();
        var content = command.Contents.Single() with
        {
            Price = 100m,
            PriceWithDiscount = 101m
        };

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(command with { Contents = [content] }));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0.001)]
    public async Task CreateSale_WithInvalidPayedSum_ThrowsValidationException(decimal payedSum)
    {
        var command = CreateValidCommand() with { PayedSum = payedSum };

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreateSale_WithEmptyBuyerId_ThrowsValidationException()
    {
        var command = CreateValidCommand() with { BuyerId = Guid.Empty };

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreateSale_WithMissingCurrency_ThrowsDbValidationException()
    {
        var command = CreateValidCommand() with { CurrencyId = int.MaxValue };

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.CurrencyNotFound);
    }

    [Fact]
    public async Task CreateSale_WithMissingStorage_ThrowsDbValidationException()
    {
        var command = CreateValidCommand() with { StorageName = "missing-storage" };

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.StoragesNotFound);
    }

    [Fact]
    public async Task CreateSale_WithMissingBuyer_ThrowsDbValidationException()
    {
        var command = CreateValidCommand() with { BuyerId = Guid.NewGuid() };

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.UsersNotFound);
    }

    [Fact]
    public async Task CreateSale_WithMissingProduct_ThrowsDbValidationException()
    {
        var command = CreateValidCommand();
        var content = command.Contents.Single() with { ProductId = int.MaxValue };

        var exception = await Assert.ThrowsAsync<DbValidationException>(() =>
            Mediator.Send(command with { Contents = [content] }));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.ArticlesNotFound);
    }

    private CreateSaleCommand CreateValidCommand()
    {
        var buyer = Buyer();
        var storageContent = StorageContent();

        return CreateCommand(
            buyer.Id,
            storageContent.CurrencyId,
            storageContent.StorageName,
            [NewContent(storageContent.ProductId, count: 1)]);
    }

    private CreateSaleCommand CreateCommand(
        Guid buyerId,
        int currencyId,
        string storageName,
        IEnumerable<NewSaleContentDto> contents,
        decimal? payedSum = null,
        string? confirmationCode = null)
    {
        return new CreateSaleCommand(
            buyerId,
            currencyId,
            storageName,
            DateTime.UtcNow,
            contents.ToList(),
            Faker.Lorem.Sentence(),
            payedSum,
            confirmationCode);
    }

    private static NewSaleContentDto NewContent(
        int productId,
        int count,
        decimal price = 100m,
        decimal priceWithDiscount = 90m)
    {
        return new NewSaleContentDto
        {
            ProductId = productId,
            Count = count,
            Price = price,
            PriceWithDiscount = priceWithDiscount
        };
    }

    private User Buyer()
    {
        return GetContext<UsersTestContext>().Users.First();
    }

    private StorageContent StorageContent()
    {
        return GetContext<StorageContentTestContext>().StorageContents.First(x => x.Count > 0);
    }

    private StorageContent StorageContentWithCountAtLeast(int count)
    {
        return GetContext<StorageContentTestContext>().StorageContents
            .Where(x => x.Count > 0)
            .GroupBy(x => new { x.ProductId, x.StorageName })
            .First(x => x.Sum(z => z.Count) >= count)
            .First();
    }

    private Task<int> CountOnStorage(StorageContent storageContent)
    {
        return Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == storageContent.ProductId && x.StorageName == storageContent.StorageName)
            .SumAsync(x => x.Count);
    }

}
