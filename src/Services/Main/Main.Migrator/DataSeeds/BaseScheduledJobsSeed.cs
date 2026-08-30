using Main.Persistence.Context;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class BaseScheduledJobsSeed : ISeed<DContext>
{
	public Task SeedAsync(DContext context) =>
		Task.CompletedTask; //TODO: we need to seed basic lrts like balances recalculation.

	public int GetPriority() => int.MaxValue;
}
