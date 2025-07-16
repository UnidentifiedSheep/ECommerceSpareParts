using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Exceptions.Sales;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Sale;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.ServicesTests.SalesTests;

[Collection("Combined collection")]
public class DeleteSaleTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly ISale _sale;
    
    private Currency _currency = null!;
    private AspNetUser _user = null!;
    private Transaction _transaction = null!;
    private Storage _storage = null!;
    private MonoliteUnicorn.PostGres.Main.Sale _saleModel = null!;
    
    public DeleteSaleTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _sale = sp.GetRequiredService<ISale>();
    }
        
    public async Task InitializeAsync()
    {
        var currencies = await _context.AddMockCurrency(1);
        await _context.AddMockProducersAndArticles();
        _currency = currencies.Single();
        _user = await _context.AddMockUser();
        _storage = await _context.AddMockStorage();
        var receiver = await _context.AddMockUser();
        _transaction = (await _context.AddMockTransaction([receiver.Id], [_user.Id], _user.Id,
            [_currency.Id], 1)).Single();
        _saleModel = await _context.AddMockSale(_transaction.Id, _user.Id, _user.Id, _storage.Name, _currency.Id);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeleteSale_InvalidSaleId_ThrowsSaleNotFoundException()
    {
        await Assert.ThrowsAsync<SaleNotFoundException>(async () => await _sale.DeleteSale("not-existing-sale", _user.Id));
    }
    
    [Fact]
    public async Task DeleteSale_InvalidUserId_ThrowsUserNotFoundException()
    {
        await Assert.ThrowsAsync<UserNotFoundException>(async () => await _sale.DeleteSale(_saleModel.Id, "not-existing-user"));
    }
    
    [Fact]
    public async Task DeleteSale_WithValidData_Succeeds()
    {
        var sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == _saleModel.Id);
        Assert.NotNull(sale);
        await _sale.DeleteSale(_saleModel.Id, _user.Id);
        sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == _saleModel.Id);
        Assert.Null(sale);
    }
}