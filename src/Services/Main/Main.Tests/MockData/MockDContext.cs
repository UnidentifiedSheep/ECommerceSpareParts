using Main.Entities;
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
        await context.Database.ExecuteSqlAsync($"""
                                                   INSERT INTO article_crosses (article_id, article_cross_id) 
                                                   values ({leftId}, {rightId})
                                                   """);
    }
    

    public static async Task AddMockCurrencies(this DContext context)
    {
        var existingIds = await context.Currencies
            .AsNoTracking()
            .Select(x => x.Id)
            .ToHashSetAsync();

        var existingCurrencyToUsdIds = await context.CurrencyToUsds
            .AsNoTracking()
            .Select(x => x.CurrencyId)
            .ToHashSetAsync();

        var toAdd = new List<Currency>()
        {
            new()
            {
                Id = 3,
                ShortName = "Руб.",
                Name = "Рубль",
                CurrencySign = "₽",
                Code = "RUB",
                CurrencyToUsd = new CurrencyToUsd
                {
                    CurrencyId = 3,
                    ToUsd = 80.400696m
                }
            },
            new()
            {
                Id = 2,
                ShortName = "Лиры",
                Name = "Турецкая лира",
                CurrencySign = "₺",
                Code = "TRY",
                CurrencyToUsd = new CurrencyToUsd
                {
                    CurrencyId = 2,
                    ToUsd = 41.145504m
                }
            },
            new()
            {
                Id = 1,
                ShortName = "Дол.",
                Name = "Доллар США",
                CurrencySign = "$",
                Code = "USD",
                CurrencyToUsd = new CurrencyToUsd
                {
                    CurrencyId = 1,
                    ToUsd = 1m
                }
            },
            new()
            {
                Id = 4,
                ShortName = "Евро",
                Name = "Евро",
                CurrencySign = "€",
                Code = "EUR",
                CurrencyToUsd = new CurrencyToUsd
                {
                    CurrencyId = 4,
                    ToUsd = 0.85493m
                }
            }
        };

        var newCurrencies = toAdd.Where(c => !existingIds.Contains(c.Id)).ToList();

        foreach (var c in newCurrencies)
        {
            if (c.CurrencyToUsd != null &&
                existingCurrencyToUsdIds.Contains(c.CurrencyToUsd.CurrencyId))
            {
                c.CurrencyToUsd = null;
            }
        }

        if (!newCurrencies.Any()) return;

        await context.AddRangeAsync(newCurrencies);
        await context.SaveChangesAsync();

        foreach (var currency in newCurrencies)
        {
            context.Entry(currency).State = EntityState.Detached;
            if (currency.CurrencyToUsd != null)
                context.Entry(currency.CurrencyToUsd).State = EntityState.Detached;
        }
    }
}