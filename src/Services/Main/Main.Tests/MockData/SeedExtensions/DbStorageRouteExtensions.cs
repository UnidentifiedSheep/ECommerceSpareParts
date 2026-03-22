using Main.Entities;
using Main.Persistence.Context;
using Tests.MockData.DataFactories;

namespace Tests.MockData.SeedExtensions;

public static class DbStorageRouteExtensions
{
    /// <summary>
    /// Generates 1 active and other not active storage routes.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="count"></param>
    /// <param name="storageNames"></param>
    /// <param name="userIds"></param>
    /// <param name="currencyIds"></param>
    /// <returns></returns>
    public static async Task<List<StorageRoute>> CreateStorageRoutes(this DContext context, int count, 
        IEnumerable<string> storageNames, IEnumerable<Guid> userIds, IEnumerable<int> currencyIds)
    {
        var routes = StorageRouteFactory.Create(count, storageNames, userIds, currencyIds);
        routes.First().IsActive = true;
        for (int i = 1; i < routes.Count; i++) routes[i].IsActive = false;
        
        await context.AddRangeAsync(routes);
        await context.SaveChangesAsync();

        return routes;
    }
}