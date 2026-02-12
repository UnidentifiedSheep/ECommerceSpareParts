using Enums;
using Main.Entities;
using Main.Enums;
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

    public static async Task AddMockWeightsAndSizesToArticles(this DbContext context, IEnumerable<int>? ids = null)
    {
        var articleIds = ids ?? await context.Set<Main.Entities.Article>()
            .Select(a => a.Id)
            .ToListAsync();

        var sizes = new List<ArticleSize>();
        var weights = new List<ArticleWeight>();
        foreach (var articleId in articleIds)
        {
            var weight = Math.Round(Global.Faker.Random.Decimal(0.1m, 10m), 2);
            var length = Math.Round(Global.Faker.Random.Decimal(1m, 100m), 2);
            var width = Math.Round(Global.Faker.Random.Decimal(1m, 100m), 2);
            var height = Math.Round(Global.Faker.Random.Decimal(1m, 100m), 2);

            sizes.Add(new()
            {
                ArticleId = articleId,
                Length = length,
                Width = width,
                Height = height,
                VolumeM3 = length * width * height
            });
            
            weights.Add(new ArticleWeight()
            {
                ArticleId = articleId,
                Weight = weight,
                Unit = WeightUnit.Kilogram
            });
        }
        
        await context.AddRangeAsync(sizes);
        await context.AddRangeAsync(weights);
        await context.SaveChangesAsync();
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