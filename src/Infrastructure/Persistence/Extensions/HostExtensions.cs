using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Persistence.Extensions;

public static class HostExtensions
{
    public static async Task<IHost> EnsureDbExists<TContext>(this IHost host) where TContext : DbContext
    {
        await using var scope = host.Services.CreateAsyncScope();
        DbContext context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.EnsureCreatedAsync();
        return host;
    }
}