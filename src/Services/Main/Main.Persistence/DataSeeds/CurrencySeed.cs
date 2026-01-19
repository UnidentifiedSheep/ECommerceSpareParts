using Main.Entities;
using Main.Persistence.Context;
using Persistence.Interfaces;

namespace Main.Persistence.DataSeeds;

public class CurrencySeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        var usd = new Currency
        {
            Id = 3,
            ShortName = "Дол.",
            Name = "Доллар США",
            CurrencySign = "$",
            Code = "USD"
        };
        
        await context.Currencies.AddAsync(usd);
        await context.SaveChangesAsync();
    }

    public int GetPriority() => 0;
}