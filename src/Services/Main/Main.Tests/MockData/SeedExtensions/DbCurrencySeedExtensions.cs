using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Tests.MockData.SeedExtensions;

public static class DbCurrencySeedExtensions
{
    public static async Task<List<Currency>> CreateCurrencies(this DContext ctx)
    {
        await ctx.Database.ExecuteSqlRawAsync("""
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
        return await ctx.Currencies.ToListAsync();
    }
}