using Bogus;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator;

namespace Tests.MockData;

public static class MockDContext
{
    public static Faker Faker => new(MockData.Locale); 
    public static async Task AddMockProducersAndArticles(this DContext context)
    {
        await context.Producers.AddRangeAsync(
            new Producer { Name = Faker.Lorem.Letter(30) },
            new Producer { Name = Faker.Lorem.Letter(30) },
            new Producer { Name = Faker.Lorem.Letter(30) },
            new Producer { Name = Faker.Lorem.Letter(30) },
            new Producer { Name = Faker.Lorem.Letter(30) });
        await context.SaveChangesAsync();
        var producers = await context.Producers.ToListAsync();
        var producerIds = producers.Select(x => x.Id).ToList();
        await context.Articles.AddRangeAsync(
            new Article { ArticleNumber = "202.213", NormalizedArticleNumber = "202213", ArticleName = "Диск выжим", ProducerId = Faker.PickRandom(producerIds)},
            new Article { ArticleNumber = "50555", NormalizedArticleNumber = "50555", ArticleName = "сальник 239x88", ProducerId = Faker.PickRandom(producerIds)},
            new Article { ArticleNumber = "505 523 99 00", NormalizedArticleNumber = "5055239900", ArticleName = "подшипник 239x88", ProducerId = Faker.PickRandom(producerIds)},
            new Article { ArticleNumber = "aKb.21-12", NormalizedArticleNumber = "AKB2112", ArticleName = "рессора 239x88", ProducerId = Faker.PickRandom(producerIds)},
            new Article { ArticleNumber = "aKb.21-11", NormalizedArticleNumber = "AKB2111", ArticleName = "рессора 239x78", ProducerId = Faker.PickRandom(producerIds)},
            new Article { ArticleNumber = "202.213", NormalizedArticleNumber = "202213", ArticleName = "Диск выжим", ProducerId = Faker.PickRandom(producerIds)});
        await context.SaveChangesAsync();
    }

    public static async Task<AspNetUser> CreateSystemUser(this DContext context)
    {
        var systemModel = new AspNetUser
        {
            Id = "SYSTEM",
            Name = "SYSTEM",
            Surname = "SYSTEM",
            Email = "SYSTEM",
            UserName = "SYSTEM",
            NormalizedEmail = "SYSTEM",
            NormalizedUserName = "SYSTEM",
            PhoneNumberConfirmed = false,
            EmailConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            IsSupplier = false
        };
        await context.AspNetUsers.AddAsync(systemModel);
        await context.SaveChangesAsync();
        return systemModel;
    }

    public static async Task<AspNetUser> AddMockUser(this DContext context)
    {
        var user = MockData.CreateNewUser(1)[0];
        user.Id = Guid.NewGuid().ToString();
        await context.AspNetUsers.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    public static async Task<Storage> AddMockStorage(this DContext context)
    {
        var storage = MockData.CreateNewStorage(1)[0];
        await context.Storages.AddAsync(storage);
        await context.SaveChangesAsync();
        return storage;
    }

    public static async Task<IEnumerable<Storage>> AddMockStorages(this DContext context, int count)
    {
        var storages = MockData.CreateNewStorage(count);
        await context.Storages.AddRangeAsync(storages);
        await context.SaveChangesAsync();
        return storages;
    }

    public static async Task<IEnumerable<StorageContent>> AddMockStorageContent(this DContext context, int count)
    {
        var articleIds = await context.Articles.Select(x => x.Id).ToListAsync();
        var storages = await context.Storages.Select(x => x.Name).ToListAsync();
        var currencies = await context.Currencies.Select(x => x.Id).ToListAsync();
        var storageContent = MockData.CreateStorageContent(articleIds, storages, currencies, count);
        foreach (var item in storageContent)
        {
            var article = await context.Articles.FirstAsync(x => x.Id == item.ArticleId);
            article.TotalCount += item.Count;
        }
        await context.StorageContents.AddRangeAsync(storageContent);
        await context.SaveChangesAsync();
        return storageContent;
    }

    public static async Task<IEnumerable<Currency>> AddMockCurrency(this DContext context, int count)
    {
        var currencies = MockData.CreateCurrency(count);
        await context.Currencies.AddRangeAsync(currencies);
        await context.SaveChangesAsync();
        var dict = new Dictionary<int, decimal>();
        foreach (var currency in currencies)
            dict[currency.Id] = Math.Round(Faker.Random.Decimal(1.01m, 200));
        dict[3] = 1m;
        CurrencyConverter.LoadRates(dict);
        return currencies;
    }

    private static readonly string[] Tables =
    [
        "producer", "producer_details", "producers_other_names",
        "AspNetRoles", "AspNetRoleClaims", "AspNetUsers", "AspNetUserClaims",
        "AspNetUserLogins", "AspNetUserRoles", "articles", "categories",
        "articles_content", "articles_pair", "storage_content", "article_characteristics",
        "article_images", "storages", "currency", "transactions",
        "user_balances", "user_discounts", "currency_history", "currency_to_usd",
        "default_settings", "markup_group", "markup_ranges", "purchase", "purchase_content",
        "buy_sell_prices", "sale_content", "categories", "sale",
        "article_crosses", "article_ean", "article_supplier_buy_info", "transaction_versions",
        "user_mails", "user_vehicles", "sale_content_details", "storage_movement", "user_search_history", "storage_content_reservations",
    ];
    public static async Task ClearDatabaseFull(this DContext context)
    {
        var tablesList = string.Join(", ", Tables.Select(t => $@"""{t}"""));
        var sql = $@"TRUNCATE TABLE {tablesList} RESTART IDENTITY CASCADE";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public static async Task<IEnumerable<Transaction>> AddMockTransaction(this DContext context,
        IEnumerable<string> receiverIds, IEnumerable<string> senderIds, string whoMade, IEnumerable<int> currencyIds,
        int count)
    {
        var transactions = MockData.CreateTransaction(receiverIds, senderIds, whoMade, currencyIds, count);
        await context.Transactions.AddRangeAsync(transactions);
        foreach (var item in transactions)
        {
            var receiverBalance = await context.UserBalances
                .FirstOrDefaultAsync(x => x.UserId == item.ReceiverId && x.CurrencyId == item.CurrencyId);
            var senderBalance = await context.UserBalances
                .FirstOrDefaultAsync(x => x.UserId == item.ReceiverId && x.CurrencyId == item.CurrencyId);

            if (receiverBalance == null)
            {
                receiverBalance = new UserBalance
                {
                    UserId = item.ReceiverId,
                    CurrencyId = item.CurrencyId,
                    Balance = 0
                };
                await context.UserBalances.AddAsync(receiverBalance);
            }

            if (senderBalance == null)
            {
                senderBalance = new UserBalance
                {
                    UserId = item.SenderId,
                    CurrencyId = item.CurrencyId,
                    Balance = 0
                };
                await context.UserBalances.AddAsync(senderBalance);
            }
            
            senderBalance.Balance -= item.TransactionSum;
            receiverBalance.Balance += item.TransactionSum;
            await context.SaveChangesAsync();
        }
        return transactions;
    }
}