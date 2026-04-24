using BulkValidation.Core.Exceptions;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;
using Main.Application.Handlers.StorageRoutes.AddStorageRoute;
using Main.Entities;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Purchase;
using Main.Entities.Storage;
using Main.Entities.Transaction;
using Main.Enums;
using Main.Persistence.Context;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.MockData;

namespace Tests.HandlersTests.Purchases;

[Collection("Combined collection")]
public class UpsertPurchaseLogisticsTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private Transaction _deliveryTransaction = null!;

    private Purchase _purchase = null!;
    private StorageRoute _route = null!;

    public UpsertPurchaseLogisticsTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockUser();
        await _mediator.AddMockUser();
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();

        var user = await _context.Users.FirstAsync();
        var storage = await _context.Storages.FirstAsync();
        var storageTo = await _context.Storages.FirstAsync(x => x.Name != storage.Name);
        var article = await _context.Products.FirstAsync();
        var currency = await _context.Currencies.FirstAsync();

        await _mediator.AddMockStorageContents([article.Id], currency.Id, storage.Name, user.Id);

        var receiver = await _context.Users.FirstAsync(x => x.Id != user.Id);

        var transaction = await _mediator.AddMockTransaction(user.Id, receiver.Id, user.Id, 1000m);
        _deliveryTransaction = await _mediator.AddMockTransaction(user.Id, receiver.Id, user.Id, 1234m);

        var storageContent = (await _context.StorageContents.FirstAsync()).Adapt<StorageContentDto>();

        var content = new List<(NewPurchaseContentDto, int?)>
        {
            (new NewPurchaseContentDto
            {
                ArticleId = article.Id,
                Count = 10,
                Price = 100.50m
            }, storageContent.Id)
        };

        var createPurchaseCommand = new CreatePurchaseCommand(content, currency.Id, "Test Comment",
            user.Id, transaction.Id, storage.Name, user.Id, DateTime.Now);
        _purchase = (await _mediator.Send(createPurchaseCommand)).Purchase;

        var addRouteCommand = new AddStorageRouteCommand(storage.Name, storageTo.Name, 1000,
            RouteType.IntraCity, LogisticPricingType.PerOrder, 60, 10.5m, 20.5m,
            currency.Id, 5.0m, null, user.Id);

        var routeId = (await _mediator.Send(addRouteCommand)).RouteId;
        _route = await _context.StorageRoutes.FirstAsync(x => x.Id == routeId);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabase();
    }

    [Fact]
    public async Task Create_WithValidData_Succeeds()
    {
        var command = new UpsertPurchaseLogisticsCommand(_purchase.Id, _route.Id,
            _deliveryTransaction.Id, false);
        await _mediator.Send(command);

        var dbValue = await GetLogistic();

        Assert.NotNull(dbValue);
        AssertFieldValid(dbValue, false, _deliveryTransaction.Id);
    }

    [Fact]
    public async Task Update_WithValidData_Succeeds()
    {
        var command = new UpsertPurchaseLogisticsCommand(_purchase.Id, _route.Id,
            _deliveryTransaction.Id, false);
        await _mediator.Send(command);
        await _mediator.Send(command with { MinimumPriceApplied = true });

        var dbValue = await GetLogistic();

        Assert.NotNull(dbValue);
        AssertFieldValid(dbValue, true, _deliveryTransaction.Id);
    }

    [Fact]
    public async Task Create_WithNullTransaction_Succeeds()
    {
        var command = new UpsertPurchaseLogisticsCommand(_purchase.Id, _route.Id, null, false);
        await _mediator.Send(command);

        var dbValue = await GetLogistic();

        Assert.NotNull(dbValue);
        AssertFieldValid(dbValue, false, null);
    }

    [Fact]
    public async Task Create_WithInvalidRoute_FailsDbValidation()
    {
        var command = new UpsertPurchaseLogisticsCommand(_purchase.Id, Guid.NewGuid(), null, false);
        await Assert.ThrowsAsync<StorageRouteNotFound>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task Create_WithTransactionId_FailsDbValidation()
    {
        var command = new UpsertPurchaseLogisticsCommand(_purchase.Id, _route.Id, Guid.NewGuid(), false);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task Create_WithPurchaseId_FailsDbValidation()
    {
        var command = new UpsertPurchaseLogisticsCommand(Guid.NewGuid().ToString(), _route.Id, null, false);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    private void AssertFieldValid(PurchaseLogistic dbValue, bool minPriceApplied, Guid? deliveryTransactionId)
    {
        Assert.Equal(_purchase.Id, dbValue.PurchaseId);
        Assert.Equal(deliveryTransactionId, dbValue.TransactionId);

        Assert.Equal(_route.CurrencyId, dbValue.CurrencyId);
        Assert.Equal(_route.Id, dbValue.RouteId);
        Assert.Equal(_route.RouteType, dbValue.RouteType);
        Assert.Equal(_route.PriceKg, dbValue.PriceKg);
        Assert.Equal(_route.PricePerM3, dbValue.PricePerM3);
        Assert.Equal(_route.PricePerOrder, dbValue.PricePerOrder);
        Assert.Equal(_route.PricingModel, dbValue.PricingModel);
        Assert.Equal(_route.MinimumPrice, dbValue.MinimumPrice);
        Assert.Equal(minPriceApplied, dbValue.MinimumPriceApplied);
    }

    private async Task<PurchaseLogistic?> GetLogistic()
    {
        return await _context.PurchaseLogistics
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PurchaseId == _purchase.Id);
    }
}