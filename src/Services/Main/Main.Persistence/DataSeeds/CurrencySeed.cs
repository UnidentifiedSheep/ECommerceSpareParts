using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Persistence.DataSeeds;

public class CurrencySeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        if (await context.Currencies.AnyAsync(x => x.Id == 1))
            return;
        
        var usd = new Currency
        {
            Id = 1,
            ShortName = "Дол.",
            Name = "Доллар США",
            CurrencySign = "$",
            Code = "USD"
        };

        await context.Currencies.AddAsync(usd);
        await context.SaveChangesAsync();
    }

    public int GetPriority()
    {
        return 0;
    }
}