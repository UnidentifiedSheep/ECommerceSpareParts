using Microsoft.EntityFrameworkCore;

namespace Persistence.Interfaces;

public interface ISeed<TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
    /// <summary>
    /// Priority of the seed.
    /// Seeds with lower priority will be executed first.
    /// </summary>
    /// <returns>Priority</returns>
    int GetPriority();
}