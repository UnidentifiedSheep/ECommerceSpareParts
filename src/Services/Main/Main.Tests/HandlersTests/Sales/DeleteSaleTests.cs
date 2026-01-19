using Exceptions.Exceptions.Sales;
using Main.Application.Configs;
using Main.Application.Handlers.Sales.DeleteSale;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Sales;

[Collection("Combined collection")]
public class DeleteSaleTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private Currency _currency = null!;
    private Sale _saleModel = null!;
    private Storage _storage = null!;
    private Transaction _transaction = null!;
    private User _user = null!;

    public DeleteSaleTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
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
        await _context.AddMockCurrencies();

        _user = await _context.Users.FirstAsync();
        _storage = await _context.Storages.FirstAsync();
        var articleIds = await _context.Articles.Select(a => a.Id).ToListAsync();
        var storage = await _context.Storages.FirstAsync();
        var currency = await _context.Currencies.FirstAsync();

        await _mediator.AddMockStorageContents(articleIds, currency.Id, storage.Name, _user.Id, 10);

        var storageContents = await _context.StorageContents.ToListAsync();

        var receiver = await _context.Users.FirstAsync(x => x.Id != _user.Id);
        await _mediator.AddMockTransaction(_user.Id, receiver.Id, _user.Id, 1212.21m);
        _transaction = await _context.Transactions.FirstAsync();
        _currency = await _context.Currencies.FirstAsync(x => x.Id == _transaction.CurrencyId);

        await _mediator.AddMockSale(storageContents, _currency.Id, _user.Id, _transaction.Id, _storage.Name);
        _saleModel = await _context.Sales.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeleteSale_InvalidSaleId_ThrowsSaleNotFoundException()
    {
        var command = new DeleteSaleCommand("not-existing-sale");
        await Assert.ThrowsAsync<SaleNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteSale_WithValidData_Succeeds()
    {
        var sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == _saleModel.Id);
        Assert.NotNull(sale);
        var command = new DeleteSaleCommand(sale.Id);
        await _mediator.Send(command);
        sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == _saleModel.Id);
        Assert.Null(sale);
    }
}