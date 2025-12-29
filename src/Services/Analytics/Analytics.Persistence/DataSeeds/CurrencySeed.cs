using Analytics.Core.Entities;
using Analytics.Persistence.Context;
using Persistence.Interfaces;

namespace Analytics.Persistence.DataSeeds;

public class CurrencySeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        var usd = new Currency
        {
            Id = 3,
            ToUsd = 0
        };
        
        await context.Currencies.AddAsync(usd);
        await context.SaveChangesAsync();
    }

    public int GetPriority() => 0;
}