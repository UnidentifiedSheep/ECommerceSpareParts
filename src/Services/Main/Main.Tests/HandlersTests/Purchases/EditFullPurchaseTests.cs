using Enums;
using Extensions;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Application.Handlers.ArticleSizes.SetArticleSizes;
using Main.Application.Handlers.ArticleWeight.SetArticleWeight;
using Main.Application.Handlers.Purchases.CreateFullPurchase;
using Main.Application.Handlers.Purchases.EditFullPurchase;
using Main.Application.Handlers.StorageRoutes.AddStorageRoute;
using Main.Entities;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Purchases;

[Collection("Combined collection")]
public class EditFullPurchaseTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private Currency _currency = null!;
    private Storage _storageTo = null!;
    private Storage _storageFrom = null!;
    private User _user = null!;
    private User _supplier = null!;
    private User _carrier = null!;
    private Article _article = null!;
    private Purchase _purchase = null!;

    public EditFullPurchaseTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockUser();
        await _mediator.AddMockUser();
        await _mediator.AddMockUser();
        await _mediator.AddMockUser();
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();

        var users = await _context.Users.Take(4).ToListAsync();
        _user = users[0];
        _supplier = users[1];
        _carrier = users[2];
        
        
        Main.Application.Global.SetSystemId(users[3].Id.ToString());

        var storages = await _context.Storages.Take(2).ToListAsync();
        _storageTo = storages[0];
        _storageFrom = storages[1];

        await _mediator.MockMapStorageToUser(_supplier.Id, _storageFrom.Name);

        _article = await _context.Articles.FirstAsync();
        _currency = await _context.Currencies.FirstAsync();

        await CreateMockPurchase();
        _purchase = await _context.Purchases
            .Include(x => x.PurchaseLogistic)
            .Include(x => x.PurchaseContents)
            .ThenInclude(x => x.PurchaseContentLogistic)
            .FirstAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    private async Task CreateMockPurchase()
    {
        await _mediator.Send(new SetArticleSizesCommand(_article.Id, 10, 10, 10, DimensionUnit.Centimeter));
        await _mediator.Send(new SetArticleWeightCommand(_article.Id, 1, WeightUnit.Kilogram));

        var result = await _mediator.Send(new AddStorageRouteCommand(
            _storageFrom.Name, _storageTo.Name, 1000, RouteType.InterCity, LogisticPricingType.PerWeight,
            60, 10m, 0m, _currency.Id, 0m, 0m, _carrier.Id));

        await MakeActive(result.RouteId);
        var content = new List<NewPurchaseContentDto>
        {
            new()
            {
                ArticleId = _article.Id,
                Count = 1,
                Price = 100m,
                CalculateLogistics = true
            },
            new()
            {
                ArticleId = _article.Id,
                Count = 3,
                Price = 100m,
                CalculateLogistics = true
            },
            new()
            {
                ArticleId = _article.Id,
                Count = 2,
                Price = 100m,
                CalculateLogistics = false
            },
            new()
            {
                ArticleId = _article.Id,
                Count = 5,
                Price = 100m,
                CalculateLogistics = false
            },
            new()
            {
                ArticleId = _article.Id,
                Count = 4,
                Price = 100m,
                CalculateLogistics = true
            }
        };

        var command = new CreateFullPurchaseCommand(
            _user.Id, _supplier.Id, _currency.Id, _storageTo.Name, DateTime.Now,
            content, "Logistics Purchase", null, true, _storageFrom.Name);

        await _mediator.Send(command);
    }

    [Fact]
    public async Task WithValidData_Succeeds()
    {
        var content = new List<EditPurchaseDto>
        {
            new()
            {
                ArticleId = _article.Id,
                Count = 1,
                Price = 100m,
                CalculateLogistics = true
            },
            new()
            {
                ArticleId = _article.Id,
                Count = 3,
                Price = 100m,
                CalculateLogistics = true
            },
            new()
            {
                ArticleId = _article.Id,
                Count = 2,
                Price = 100m,
                CalculateLogistics = false
            },
            new()
            {
                ArticleId = _article.Id,
                Count = 5,
                Price = 100m,
                CalculateLogistics = false
            },
            new()
            {
                ArticleId = _article.Id,
                Count = 4,
                Price = 100m,
                CalculateLogistics = true
            }
        };
        
        var command = new EditFullPurchaseCommand(content, _purchase.Id, _currency.Id, null, DateTime.UtcNow, 
            _user.Id, true, _storageFrom.Name);
        await _mediator.Send(command);
    }
    
    private async Task MakeActive(Guid id)
    {
        var route = await _context.StorageRoutes.FirstAsync(x => x.Id == id);
        var activeRoute = await _context.StorageRoutes
            .FirstOrDefaultAsync(x => x.FromStorageName == route.FromStorageName &&
                                      x.ToStorageName == route.ToStorageName &&
                                      x.IsActive);
        activeRoute?.IsActive = false;
        route.IsActive = true;
        await _context.SaveChangesAsync();
    }
}