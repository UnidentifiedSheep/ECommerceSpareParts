using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Tests.MockData;

public static class MockDContext
{
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
        "user_mails", "user_vehicles", "sale_content_details", "storage_movement", "user_search_history",
        "storage_content_reservations"
    ];

    public static async Task ClearDatabaseFull(this DContext context)
    {
        var tablesList = string.Join(", ", Tables.Select(t => $@"""{t}"""));
        var sql = $@"TRUNCATE TABLE {tablesList} RESTART IDENTITY CASCADE";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public static async Task AddArticleCross(this DContext context, int leftId, int rightId)
    {
        await context.Database.ExecuteSqlRawAsync($"""
                                                   INSERT INTO article_crosses (article_id, article_cross_id) 
                                                   values ({leftId}, {rightId})
                                                   """);
    }

    public static async Task CreateSystemUser(this DContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
                                                  INSERT INTO "AspNetUsers" ("Id", "Name", "Surname", "EmailConfirmed", 
                                                                             "IsSupplier", "PhoneNumberConfirmed", 
                                                                             "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount")
                                                  values ('SYSTEM', 'SYSTEM', 'SYSTEM',  true, false, false, false, false, 0);
                                                  """);
    }

    public static async Task AddMockCurrencies(this DContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
                                                  INSERT INTO currency (id, short_name, name, currency_sign, code)
                                                  values (1,'Руб.','Рубль','₽','RUB'),
                                                         (2,'Лиры','Турецкая лира','₺','TRY'),
                                                         (3,'Дол.','Доллар США','$','USD'),
                                                         (4,'Евро','Евро','€','EUR')
                                                  ON CONFLICT (id) DO NOTHING;
                                                  INSERT INTO currency_to_usd (currency_id, to_usd)
                                                  VALUES 
                                                      ((SELECT id FROM currency WHERE code = 'RUB'), 80.400696),
                                                      ((SELECT id FROM currency WHERE code = 'TRY'), 41.145504),
                                                      ((SELECT id FROM currency WHERE code = 'USD'), 1),
                                                      ((SELECT id FROM currency WHERE code = 'EUR'), 0.85493)
                                                  ON CONFLICT (currency_id) DO NOTHING;
                                                  """);
    }
}