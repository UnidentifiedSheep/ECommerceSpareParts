using Analytics.Entities;
using Analytics.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Analytics.Persistence.DataSeeds;

public class CurrencySeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        if (await context.Currencies.AnyAsync(x => x.Id == 1))
            return;
        var usd = new Currency
        {
            Id = 1,
            ToUsd = 0
        };

        await context.Currencies.AddAsync(usd);
        await context.SaveChangesAsync();
    }

    public int GetPriority()
    {
        return 0;
    }
}