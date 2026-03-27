using Main.Entities;
using Main.Persistence.Context;
using Tests.MockData.DataFactories;

namespace Tests.MockData.SeedExtensions;

public static class DbProducerSeedExtensions
{
    public static async Task<List<Producer>> CreateProducers(this DContext ctx, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var producers = ProducerFactory.Create(count);
        await ctx.AddRangeAsync(producers);
        await ctx.SaveChangesAsync();

        return producers;
    }
}