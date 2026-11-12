using Main.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class CurrencySeed : ISeed<UnifiedDbContext>
{
	public async Task SeedAsync(UnifiedDbContext context)
	{
		if (await context.Set<Currency>().AnyAsync(x => x.Code == "USD"))
			return;

		var usd = Currency.Create(
			"Доллар США",
			"Дол.",
			"$",
			"USD");

		await context.Set<Currency>().AddAsync(usd);
		await context.SaveChangesAsync();
	}

	public int ExecutionOrder => 0;
}
