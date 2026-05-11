using Main.Entities.Currency;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class CurrencySeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        if (await context.Currencies.AnyAsync(x => x.Id == 1))
            return;

        var usd = Currency.Create("Доллар США", "Дол.", "$", "USD");

        await context.Currencies.AddAsync(usd);
        await context.SaveChangesAsync();
    }

    public int GetPriority()
    {
        return 0;
    }
}