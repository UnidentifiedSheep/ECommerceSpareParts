using FluentAssertions;
using Main.Application.Handlers.Purchases.CreateFullPurchase;
using Main.Entities;
using Main.Entities.Product;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.MockData.DataFactories.Purchase;
using Tests.TestContexts;

namespace Tests.HandlersTests.Purchases;

[Collection("Combined collection")]
public class CreateFullPurchaseTests : IAsyncLifetime
{
    private readonly PurchaseTestContext _testContext;

    public CreateFullPurchaseTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _testContext = sp.GetRequiredService<PurchaseTestContext>();
    }

    public async Task InitializeAsync()
    {
        await _testContext.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _testContext.DbContext.ClearDatabase();
    }

    [Fact]
    public async Task CreateFullPurchase_WithoutLogistics_Succeeds()
    {
        var content = NewPurchaseContentDtoFactory.Create(
            2,
            _testContext.Articles.Select(x => x.Id));
        content.ForEach(x =>
        {
            x.CalculateLogistics = false;
            x.Count = 1;
        });
        var totalSum = content.Sum(x => x.Count * x.Price);

        var command = new CreateFullPurchaseCommand(
            _testContext.User.Id, _testContext.Supplier.Id, _testContext.Currency.Id, _testContext.StorageTo.Name,
            DateTime.Now, content, "Full Purchase Comment", null, false, null);


        var task = async () => await _testContext.Mediator.Send(command);
        await task.Should().NotThrowAsync();

        var purchases = await GetPurchasesAsync();

        purchases.Should().HaveCount(1);

        var purchase = purchases[0];
        purchase.Storage.Should().Be(_testContext.StorageTo.Name);
        purchase.PurchaseContents.Should().HaveCount(content.Count);

        var transactions = await GetTransactionsAsync();
        transactions.Should().HaveCount(1);

        var transaction = transactions[0];

        transaction.SenderId.Should().Be(_testContext.Supplier.Id);
        transaction.ReceiverId.Should().Be(Main.Application.Global.SystemId);
        transaction.TransactionSum.Should().Be(totalSum);
    }

    [Fact]
    public async Task CreateFullPurchase_WithPayment_Succeeds()
    {
        var content = NewPurchaseContentDtoFactory.Create(
            1,
            _testContext.Articles.Select(x => x.Id));

        var totalSum = content.Sum(x => x.Count * x.Price);

        var command = new CreateFullPurchaseCommand(
            _testContext.User.Id, _testContext.Supplier.Id, _testContext.Currency.Id, _testContext.StorageTo.Name,
            DateTime.Now,
            content, "Purchase with payment", 500m, false, null);

        var task = async () => await _testContext.Mediator.Send(command);

        await task.Should().NotThrowAsync();

        var transactions = await GetTransactionsAsync();

        transactions.Should().HaveCount(2);
        transactions.Should().Contain(x => x.TransactionSum == 500m && x.Status == TransactionStatus.Normal);
        transactions.Should().Contain(x => x.TransactionSum == totalSum && x.Status == TransactionStatus.Purchase);
    }

    [Fact]
    public async Task CreateFullPurchase_WithLogistics_Succeeds()
    {
        var content = NewPurchaseContentDtoFactory.Create(
            5,
            _testContext.Articles.Select(x => x.Id));

        content.ForEach(x => x.CalculateLogistics = true);

        var totalSum = content.Sum(x => x.Count * x.Price);

        var command = new CreateFullPurchaseCommand(
            _testContext.User.Id, _testContext.Supplier.Id, _testContext.Currency.Id, _testContext.StorageTo.Name,
            DateTime.Now,
            content, "Logistics Purchase", null, true, _testContext.StorageFrom.Name);

        var task = async () => await _testContext.Mediator.Send(command);

        await task.Should().NotThrowAsync();

        var purchases = await GetPurchasesAsync();
        purchases.Should().HaveCount(1);

        var purchase = purchases[0];

        purchase.PurchaseLogistic.Should().NotBeNull();
        purchase.PurchaseLogistic.TransactionId.Should().NotBeNull();

        var logisticsNotNull = purchase
            .PurchaseContents
            .Where(x => x.PurchaseContentLogistic != null)
            .ToList();

        logisticsNotNull.Count.Should().Be(content.Count);

        var index = 0;
        foreach (var con in content)
        {
            if (!con.CalculateLogistics) continue;
            var purchaseContent = logisticsNotNull[index];
            var logistic = purchaseContent.PurchaseContentLogistic;

            logistic.Should().NotBeNull();
            con.Count.Should().Be(purchaseContent.Count);

            logistic.AreaM3.Should().BeGreaterThan(0);
            logistic.Price.Should().BeGreaterThan(0);
            index++;
        }

        var transactions = await GetTransactionsAsync();
        transactions.Should().HaveCount(2);

        transactions.Should().Contain(x => x.TransactionSum == totalSum && x.Status == TransactionStatus.Purchase);
        transactions.Should().Contain(x => x.Status == TransactionStatus.Logistics &&
                                           x.ReceiverId == _testContext.Carrier.Id);
    }

    private async Task<List<Purchase>> GetPurchasesAsync()
    {
        return await _testContext.DbContext.Purchases
            .Include(x => x.PurchaseLogistic)
            .Include(x => x.PurchaseContents)
            .ThenInclude(x => x.PurchaseContentLogistic)
            .ToListAsync();
    }

    private async Task<List<Transaction>> GetTransactionsAsync()
    {
        return await _testContext.DbContext.Transactions
            .ToListAsync();
    }

    private async Task<List<Product>> GetArticlesAsync()
    {
        return await _testContext.DbContext.Products
            .ToListAsync();
    }
}