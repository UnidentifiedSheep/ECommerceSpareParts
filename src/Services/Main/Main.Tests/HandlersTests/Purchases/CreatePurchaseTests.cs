using Enums;
using FluentAssertions;
using Main.Application.Dtos.Purchase;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Static;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.User;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Purchases;

public class CreatePurchaseTests : IntegrationTest
{
    private User _supplier = null!;

    public CreatePurchaseTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
        RegisterBasicContext<CurrencyRatesTestContext>();
        RegisterBasicContext<StorageTestContext>();
        RegisterBasicContext<StorageRouteTestContext>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _supplier = await new SupplierUserBuilder(Faker)
            .BuildAndAddToDb(Context);
    }

    [Fact]
    public async Task CreatePurchase_WithoutLogistics_CreatesPurchaseAndStorageContents()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var storage = GetContext<StorageTestContext>().Storages.First();
        var products = GetContext<ProductTestContext>().Products.Take(2).ToList();
        var command = CreateCommand(
            _supplier.Id,
            currency.Id,
            storage.Name,
            false,
            null,
            products.Select((product, index) => NewContent(product, index + 1, false)));

        await Mediator.Send(command);

        var purchase = await Context.Purchases
            .Include(x => x.Contents)
            .Include(x => x.PurchaseLogistic)
            .AsNoTracking()
            .SingleAsync();

        purchase.SupplierId.Should().Be(_supplier.Id);
        purchase.CurrencyId.Should().Be(currency.Id);
        purchase.Storage.Should().Be(storage.Name);
        purchase.State.Should().Be(PurchaseState.Completed);
        purchase.Comment.Should().Be(command.Comment);
        purchase.PurchaseLogistic.Should().BeNull();
        purchase.Contents.Should().HaveCount(command.PurchaseContent.Count());

        var storageContents = await Context.StorageContents.AsNoTracking().ToListAsync();
        storageContents.Should().HaveCount(command.PurchaseContent.Count());
        foreach (var item in command.PurchaseContent)
            storageContents.Should().Contain(x =>
                x.ProductId == item.ProductId &&
                x.Count == item.Count &&
                x.BuyPrice == item.Price &&
                x.CurrencyId == command.CurrencyId &&
                x.StorageName == command.StorageName);

        var purchaseTransaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == purchase.TransactionId);
        purchaseTransaction.SenderId.Should().Be(_supplier.Id);
        purchaseTransaction.ReceiverId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
        purchaseTransaction.Amount.Should().Be(command.PurchaseContent.Sum(x => x.Price * x.Count));
        purchaseTransaction.SourceType.Should().Be(TransactionSourceType.Purchase);
    }

    [Fact]
    public async Task CreatePurchase_WhenSupplierHasNoSupplierRole_ThrowsUserIsNotInNeededRole()
    {
        var member = GetContext<UsersTestContext>().Users.First();
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var storage = GetContext<StorageTestContext>().Storages.First();
        var product = GetContext<ProductTestContext>().Products.First();
        var command = CreateCommand(
            member.Id,
            currency.Id,
            storage.Name,
            false,
            null,
            [NewContent(product, 1, false)]);

        await Assert.ThrowsAsync<UserIsNotInNeededRole>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreatePurchase_WithLogistics_CreatesPurchaseLogisticsAndContentLogistics()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var product = GetContext<ProductTestContext>().Products.First();
        await AddLogisticsDependencies(product, route, _supplier.Id);

        var command = CreateCommand(
            _supplier.Id,
            currency.Id,
            route.ToStorageName,
            true,
            route.FromStorageName,
            [NewContent(product, 2, true)]);

        await Mediator.Send(command);

        var purchase = await Context.Purchases
            .Include(x => x.PurchaseLogistic)
            .Include(x => x.Contents)
            .ThenInclude(x => x.PurchaseContentLogistic)
            .AsNoTracking()
            .SingleAsync();

        purchase.PurchaseLogistic.Should().NotBeNull();
        purchase.PurchaseLogistic!.RouteId.Should().Be(route.Id);
        purchase.PurchaseLogistic.CurrencyId.Should().Be(route.CurrencyId);
        purchase.PurchaseLogistic.PricingModel.Should().Be(route.PricingModel);
        purchase.PurchaseLogistic.RouteType.Should().Be(route.RouteType);
        purchase.PurchaseLogistic.PriceKg.Should().Be(route.PriceKg);
        purchase.PurchaseLogistic.PricePerM3.Should().Be(route.PricePerM3);
        purchase.PurchaseLogistic.PricePerOrder.Should().Be(route.PricePerOrder);
        purchase.PurchaseLogistic.MinimumPrice.Should().Be(route.MinimumPrice);

        var content = purchase.Contents.Single();
        content.PurchaseContentLogistic.Should().NotBeNull();
        content.PurchaseContentLogistic!.WeightKg.Should().BeGreaterThan(0);
        content.PurchaseContentLogistic.AreaM3.Should().BeGreaterThan(0);
        content.PurchaseContentLogistic.Price.Should().BeGreaterThanOrEqualTo(0);

        if (route.CarrierId is not null)
        {
            purchase.PurchaseLogistic.TransactionId.Should().NotBeNull();
            var logisticsTransaction = await Context.Transactions
                .AsNoTracking()
                .SingleAsync(x => x.Id == purchase.PurchaseLogistic.TransactionId);
            logisticsTransaction.SenderId.Should().Be(route.CarrierId.Value);
            logisticsTransaction.ReceiverId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
            logisticsTransaction.SourceType.Should().Be(TransactionSourceType.Logistic);
        }
    }

    [Fact]
    public async Task CreatePurchase_WithPayedSum_CreatesPaymentTransaction()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var storage = GetContext<StorageTestContext>().Storages.First();
        var product = GetContext<ProductTestContext>().Products.First();
        var payedSum = 5m;
        var command = CreateCommand(
            _supplier.Id,
            currency.Id,
            storage.Name,
            false,
            null,
            [NewContent(product, 1, false)],
            payedSum);

        await Mediator.Send(command);

        var transactions = await Context.Transactions.AsNoTracking().ToListAsync();
        transactions.Should().HaveCount(2);

        var paymentTransaction = transactions.Single(x => x.Amount == payedSum);
        paymentTransaction.SenderId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
        paymentTransaction.ReceiverId.Should().Be(_supplier.Id);
        paymentTransaction.CurrencyId.Should().Be(currency.Id);
        paymentTransaction.SourceType.Should().Be(TransactionSourceType.Manual);
    }

    [Fact]
    public async Task CreatePurchase_WithZeroPayedSum_DoesNotCreatePaymentTransaction()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var storage = GetContext<StorageTestContext>().Storages.First();
        var product = GetContext<ProductTestContext>().Products.First();
        var command = CreateCommand(
            _supplier.Id,
            currency.Id,
            storage.Name,
            false,
            null,
            [NewContent(product, 1, false)],
            0m);

        await Mediator.Send(command);

        var transactions = await Context.Transactions.AsNoTracking().ToListAsync();
        transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreatePurchase_WithLogisticsButNoContentMarkedForLogistics_DoesNotCreatePurchaseLogistic()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var product = GetContext<ProductTestContext>().Products.First();
        Context.StorageOwners.Add(StorageOwner.Create(route.FromStorageName, _supplier.Id));
        await Context.SaveChangesAsync();

        var command = CreateCommand(
            _supplier.Id,
            currency.Id,
            route.ToStorageName,
            true,
            route.FromStorageName,
            [NewContent(product, 1, false)]);

        await Mediator.Send(command);

        var purchase = await Context.Purchases
            .Include(x => x.PurchaseLogistic)
            .AsNoTracking()
            .SingleAsync();

        purchase.PurchaseLogistic.Should().BeNull();
    }

    [Fact]
    public async Task CreatePurchase_WithEmptyContent_ThrowsValidationException()
    {
        var command = CreateValidCommand() with { PurchaseContent = [] };

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreatePurchase_WithInvalidContentCount_ThrowsValidationException(int count)
    {
        var command = CreateValidCommand();
        var content = command.PurchaseContent.Single() with { Count = count };

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(command with { PurchaseContent = [content] }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.001)]
    public async Task CreatePurchase_WithInvalidContentPrice_ThrowsValidationException(decimal price)
    {
        var command = CreateValidCommand();
        var content = command.PurchaseContent.Single() with { Price = price };

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(command with { PurchaseContent = [content] }));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0.001)]
    public async Task CreatePurchase_WithInvalidPayedSum_ThrowsValidationException(decimal payedSum)
    {
        var command = CreateValidCommand() with { PayedSum = payedSum };

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreatePurchase_WithEmptySupplierId_ThrowsValidationException()
    {
        var command = CreateValidCommand() with { SupplierId = Guid.Empty };

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreatePurchase_WithLogisticsAndEmptyStorageFrom_ThrowsValidationException()
    {
        var command = CreateValidCommand() with { WithLogistics = true, StorageFrom = null };

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreatePurchase_WithMissingCurrency_ThrowsDbValidationException()
    {
        var command = CreateValidCommand() with { CurrencyId = int.MaxValue };

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.CurrencyNotFound);
    }

    [Fact]
    public async Task CreatePurchase_WithMissingStorage_ThrowsDbValidationException()
    {
        var command = CreateValidCommand() with { StorageName = Faker.Lorem.Letter(200) };

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.StoragesNotFound);
    }

    [Fact]
    public async Task CreatePurchase_WithMissingProduct_ThrowsProductNotFoundException()
    {
        var command = CreateValidCommand();
        var content = command.PurchaseContent.Single() with { ProductId = int.MaxValue };

        await Assert.ThrowsAsync<ProductNotFoundException>(() =>
            Mediator.Send(command with { PurchaseContent = [content] }));
    }

    [Fact]
    public async Task CreatePurchase_WithLogisticsAndMissingStorageOwner_ThrowsDbValidationException()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var command = CreateValidCommand() with
        {
            StorageName = route.ToStorageName,
            WithLogistics = true,
            StorageFrom = route.FromStorageName
        };

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.StorageOwnerNotFound);
    }

    [Fact]
    public async Task CreatePurchase_WithLogisticsAndMissingProductSize_ThrowsProductSizesNotFoundException()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var product = GetContext<ProductTestContext>().Products.First();
        Context.ProductWeights.Add(ProductWeight.Create(product.Id, 2m, WeightUnit.Kilogram));
        Context.StorageOwners.Add(StorageOwner.Create(route.FromStorageName, _supplier.Id));
        await Context.SaveChangesAsync();

        var command = CreateCommand(
            _supplier.Id,
            GetContext<CurrencyTestContext>().Currencies[0].Id,
            route.ToStorageName,
            true,
            route.FromStorageName,
            [NewContent(product, 1, true)]);

        await Assert.ThrowsAsync<ProductSizesNotFoundException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task CreatePurchase_WithLogisticsAndMissingProductWeight_ThrowsProductWeightNotFoundException()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var product = GetContext<ProductTestContext>().Products.First();
        Context.ProductSizes.Add(ProductSize.Create(product.Id, 1m, 1m, 1m, DimensionUnit.Meter));
        Context.StorageOwners.Add(StorageOwner.Create(route.FromStorageName, _supplier.Id));
        await Context.SaveChangesAsync();

        var command = CreateCommand(
            _supplier.Id,
            GetContext<CurrencyTestContext>().Currencies[0].Id,
            route.ToStorageName,
            true,
            route.FromStorageName,
            [NewContent(product, 1, true)]);

        await Assert.ThrowsAsync<ProductWeightNotFoundException>(() => Mediator.Send(command));
    }

    private CreatePurchaseCommand CreateCommand(
        Guid supplierId,
        int currencyId,
        string storageName,
        bool withLogistics,
        string? storageFrom,
        IEnumerable<NewPurchaseContentDto> contents,
        decimal? payedSum = null)
    {
        return new CreatePurchaseCommand(
            supplierId,
            currencyId,
            storageName,
            DateTime.UtcNow,
            contents.ToList(),
            Faker.Lorem.Sentence(),
            payedSum,
            withLogistics,
            storageFrom);
    }

    private CreatePurchaseCommand CreateValidCommand()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var storage = GetContext<StorageTestContext>().Storages.First();
        var product = GetContext<ProductTestContext>().Products.First();

        return CreateCommand(
            _supplier.Id,
            currency.Id,
            storage.Name,
            false,
            null,
            [NewContent(product, 1, false)]);
    }

    private static NewPurchaseContentDto NewContent(Product product, int count, bool calculateLogistics)
    {
        return new NewPurchaseContentDto
        {
            ProductId = product.Id,
            Count = count,
            Price = 10m + count,
            CalculateLogistics = calculateLogistics,
            Comment = $"content-{count}"
        };
    }

    private async Task AddLogisticsDependencies(Product product, StorageRoute route, Guid supplierId)
    {
        Context.ProductSizes.Add(ProductSize.Create(product.Id, 1m, 1m, 1m, DimensionUnit.Meter));
        Context.ProductWeights.Add(ProductWeight.Create(product.Id, 2m, WeightUnit.Kilogram));
        Context.StorageOwners.Add(StorageOwner.Create(route.FromStorageName, supplierId));
        await Context.SaveChangesAsync();
    }
}
