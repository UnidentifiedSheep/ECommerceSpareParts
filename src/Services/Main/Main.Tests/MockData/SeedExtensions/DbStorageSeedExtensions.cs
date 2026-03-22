using Main.Entities;
using Main.Persistence.Context;
using Tests.MockData.DataFactories;

namespace Tests.MockData.SeedExtensions;

public static class DbStorageSeedExtensions
{
    public static async Task<List<Storage>> CreateStorages(this DContext ctx, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var storages = StorageFactory.Create(count);
        await ctx.Storages.AddRangeAsync(storages);
        await ctx.SaveChangesAsync();
        return storages;
    }
}