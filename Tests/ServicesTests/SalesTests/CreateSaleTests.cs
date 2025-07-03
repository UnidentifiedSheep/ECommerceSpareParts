using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Balances;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.ServicesTests.SalesTests;

[Collection("Combined collection")]
public class CreateSaleTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly DContext _context;
    private readonly Faker _faker = new(MockData.MockData.Locale);
    private readonly IBalance _balance;
    
    private Currency _currency = null!;
    private AspNetUser _receiver = null!;
    private Transaction _transaction = null!;
    
    public CreateSaleTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _serviceProvider = sp;
        _context = sp.GetRequiredService<DContext>();
        _balance = sp.GetRequiredService<IBalance>();
    }
        
    public async Task InitializeAsync()
    {
        var currencies = await _context.AddMockCurrency(1);
        _currency = currencies.Single();
        _receiver = await _context.AddMockUser();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }
}