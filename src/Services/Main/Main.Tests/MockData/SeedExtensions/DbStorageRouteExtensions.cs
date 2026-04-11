using Main.Entities;
using Main.Entities.Storage;
using Main.Persistence.Context;
using Tests.MockData.DataFactories;

namespace Tests.MockData.SeedExtensions;

public static class DbStorageRouteExtensions
{
    public static async Task<StorageRoute> CreateStorageRoutes(
        this DContext context,
        string storageFrom,
        string storageTo,
        Guid carrierId,
        IEnumerable<int> currencyIds)
    {
        var routes = StorageRouteFactory.Create(storageFrom, storageTo, [carrierId], currencyIds);

        await context.AddRangeAsync(routes);
        await context.SaveChangesAsync();

        return routes;
    }
}