using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.PostGres.Main;

namespace Tests.MockData;

public static class MockDContext
{
    public static async Task AddMockProducersAndArticles(this DContext context)
    {
        await context.Producers.AddRangeAsync(
            new Producer { Name = "Sampa" },
            new Producer { Name = "Febi" },
            new Producer { Name = "KS" },
            new Producer { Name = "MAN" },
            new Producer { Name = "Peters" });
        await context.SaveChangesAsync();
        await context.Articles.AddRangeAsync(
            new Article { ArticleNumber = "202.213", NormalizedArticleNumber = "202213", ArticleName = "Диск выжим", ProducerId = 1},
            new Article { ArticleNumber = "50555", NormalizedArticleNumber = "50555", ArticleName = "сальник 239x88", ProducerId = 2},
            new Article { ArticleNumber = "505 523 99 00", NormalizedArticleNumber = "5055239900", ArticleName = "подшипник 239x88", ProducerId = 3},
            new Article { ArticleNumber = "aKb.21-12", NormalizedArticleNumber = "AKB2112", ArticleName = "рессора 239x88", ProducerId = 4},
            new Article { ArticleNumber = "aKb.21-11", NormalizedArticleNumber = "AKB2111", ArticleName = "рессора 239x78", ProducerId = 5},
            new Article { ArticleNumber = "202.213", NormalizedArticleNumber = "202213", ArticleName = "Диск выжим", ProducerId = 4});
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

    public static async Task<Currency> AddMockCurrency(this DContext context)
    {
        var currency = new Currency
        {
            Code = "USD",
            Name = "Доллар",
            CurrencySign = "$",
            ShortName = "Дол."
        };
        await context.Currencies.AddAsync(currency);
        await context.SaveChangesAsync();
        return currency;
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
        "user_mails", "user_vehicles", "sale_content_details", "storage_movement"
    ];
    public static async Task ClearDatabaseFull(this DContext context)
    {
        var tablesList = string.Join(", ", Tables.Select(t => $@"""{t}"""));
        var sql = $@"TRUNCATE TABLE {tablesList} RESTART IDENTITY CASCADE";
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}