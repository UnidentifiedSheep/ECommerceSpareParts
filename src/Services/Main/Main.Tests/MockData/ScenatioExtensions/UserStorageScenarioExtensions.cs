using Main.Entities;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Persistence.Context;

namespace Tests.MockData.ScenatioExtensions;

public static class UserStorageScenarioExtensions
{
    public static async Task AddStorageToUser(this DContext context, User user, Storage storage)
    {
        user.StorageOwners.Add(new StorageOwner
        {
            CreatedAt = DateTime.UtcNow,
            Storage = storage
        });
        await context.SaveChangesAsync();
    }
}