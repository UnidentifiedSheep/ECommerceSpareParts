using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Logging;

namespace Persistence.Extensions;

public static class HostExtensions
{
    public static async Task<IHost> EnsureDbExists<TContext>(this IHost host) where TContext : DbContext
    {
        await using var scope = host.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        DbContext context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.EnsureCreatedAsync();
        DatabaseEvents.DatabaseEnsuredCreated(logger, typeof(TContext).Name, null);
        return host;
    }
    
    public static async Task SeedAsync<TContext>(this IHost host) where TContext : DbContext
    {
        await using var scope = host.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        
        var seeds = scope.ServiceProvider.GetServices<ISeed<TContext>>()
            .OrderBy(x => x.GetPriority())
            .ToList();
        if (seeds.Count == 0)
        {
            SeedEvents.NoSeedsFound(logger, typeof(TContext).Name, null);
            return;
        }
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.EnsureCreatedAsync();
        DatabaseEvents.DatabaseEnsuredCreated(logger, typeof(TContext).Name, null);
        
        SeedEvents.SeedStarted(logger, typeof(TContext).Name, null);
        foreach (var seed in seeds)
            await seed.SeedAsync(context);
        SeedEvents.SeedCompleted(logger, typeof(TContext).Name, null);
    }
}