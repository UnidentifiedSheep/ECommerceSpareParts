using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Application.Extensions;
using Main.Application.Handlers.ArticleSizes.SetArticleSizes;
using Main.Application.Handlers.ArticleWeight.SetArticleWeight;
using Main.Application.Handlers.Purchases.CreateFullPurchase;
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
public class CreateFullPurchaseTests : IAsyncLifetime
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

    public CreateFullPurchaseTests(CombinedContainerFixture fixture)
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
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task CreateFullPurchase_WithoutLogistics_Succeeds()
    {
        var content = new List<NewPurchaseContentDto>
        {
            new()
            {
                ArticleId = _article.Id,
                Count = 10,
                Price = 100m,
                CalculateLogistics = false
            }
        };

        var command = new CreateFullPurchaseCommand(
            _user.Id, _supplier.Id, _currency.Id, _storageTo.Name, DateTime.Now,
            content, "Full Purchase Comment", null, false, null);

        await _mediator.Send(command);

        var purchase = await _context.Purchases
            .Include(x => x.PurchaseContents)
            .FirstOrDefaultAsync(x => x.Comment == "Full Purchase Comment");

        Assert.NotNull(purchase);
        Assert.Equal(_storageTo.Name, purchase.Storage);
        Assert.Single(purchase.PurchaseContents);
        
        var transaction = await _context.Transactions.FirstOrDefaultAsync(x => x.Id == purchase.TransactionId);
        Assert.NotNull(transaction);
        Assert.Equal(_supplier.Id, transaction.SenderId);
        Assert.Equal(Main.Application.Global.SystemId, transaction.ReceiverId);
        Assert.Equal(1000m, transaction.TransactionSum);
        
        var storageContent = await _context.StorageContents.FirstOrDefaultAsync(x => x.ArticleId == _article.Id && x.StorageName == _storageTo.Name);
        Assert.NotNull(storageContent);
        Assert.Equal(10, storageContent.Count);
    }

    [Fact]
    public async Task CreateFullPurchase_WithPayment_Succeeds()
    {
        var content = new List<NewPurchaseContentDto>
        {
            new()
            {
                ArticleId = _article.Id,
                Count = 5,
                Price = 200m
            }
        };

        var command = new CreateFullPurchaseCommand(
            _user.Id, _supplier.Id, _currency.Id, _storageTo.Name, DateTime.Now,
            content, "Purchase with payment", 500m, false, null);

        await _mediator.Send(command);

        // Should have 2 transactions: 1000m (purchase) and 500m (payment)
        var transactions = await _context.Transactions
            .Where(x => (x.SenderId == _supplier.Id && x.ReceiverId == Main.Application.Global.SystemId) ||
                        (x.SenderId == Main.Application.Global.SystemId && x.ReceiverId == _supplier.Id))
            .ToListAsync();

        Assert.Equal(2, transactions.Count);
        Assert.Contains(transactions, x => x.TransactionSum == 1000m && x.Status == TransactionStatus.Purchase);
        Assert.Contains(transactions, x => x.TransactionSum == 500m && x.Status == TransactionStatus.Normal);
    }

    [Fact]
    public async Task CreateFullPurchase_WithLogistics_Succeeds()
    {
        decimal areaM3 = DimensionExtensions.ToCubicMeters(10, 10, 10, DimensionUnit.Centimeter);
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

        var purchase = await _context.Purchases
            .Include(x => x.PurchaseContents)
                .ThenInclude(pc => pc.PurchaseContentLogistic)
            .Include(x => x.PurchaseLogistic)
            .FirstOrDefaultAsync(x => x.Comment == "Logistics Purchase");

        Assert.NotNull(purchase);
        Assert.NotNull(purchase.PurchaseLogistic);
        Assert.NotNull(purchase.PurchaseLogistic.TransactionId);
        
        var logisticsNotNull = purchase.PurchaseContents.Where(x => x.PurchaseContentLogistic != null)
            .Select(x => x.PurchaseContentLogistic)
            .ToList();
        Assert.Equal(3, logisticsNotNull.Count);
        var index = 0;
        foreach (var con in content)
        {
            if (!con.CalculateLogistics) continue;
            var logistic = logisticsNotNull[index];
            
            
            Assert.Equal(con.Count, logistic!.WeightKg);
            Assert.Equal(con.Count * areaM3, logistic.AreaM3);
            index++;
        }
        
        var logisticsTransaction = await _context.Transactions.FirstAsync(x => x.Id == purchase.PurchaseLogistic.TransactionId);
        Assert.Equal(TransactionStatus.Logistics, logisticsTransaction.Status);
        Assert.Equal(_carrier.Id, logisticsTransaction.ReceiverId);
        Assert.Equal(80, logisticsTransaction.TransactionSum);
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
