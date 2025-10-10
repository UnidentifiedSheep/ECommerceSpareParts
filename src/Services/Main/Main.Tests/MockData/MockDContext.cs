using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Tests.MockData;

public static class MockDContext
{
    public static async Task ClearDatabaseFull(this DContext context)
    {
        var sql = """
                  DO $$
                  DECLARE
                      r RECORD;
                  BEGIN
                      FOR r IN 
                          SELECT schemaname, tablename
                          FROM pg_tables
                          WHERE schemaname IN ('auth', 'public')
                      LOOP
                          EXECUTE format('TRUNCATE TABLE %I.%I RESTART IDENTITY CASCADE', r.schemaname, r.tablename);
                      END LOOP;
                  END $$;
                  """;
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public static async Task AddArticleCross(this DContext context, int leftId, int rightId)
    {
        await context.Database.ExecuteSqlRawAsync($"""
                                                   INSERT INTO article_crosses (article_id, article_cross_id) 
                                                   values ({leftId}, {rightId})
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