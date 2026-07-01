using Api.Common;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Persistence;
using Pricing.Persistence.Contexts;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
        config.AddMigratorSettingsFromJsons("pricing.settings")
            .AddMigratorSettingsFromJsons("pricing.settings", "/app/configs"));

builder.ConfigureServices((_, services) =>
{
    services.AddDatabaseOptions();

    services.AddDbContext<DContext>((sp, options) =>
    {
        var connectionString = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString;
        options.UseNpgsql(connectionString, x => x.MigrationsAssembly("Pricing.Migrator"));
    });
});

var host = builder.Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<DContext>();

await db.Database.MigrateAsync();

Console.WriteLine("Pricing migrations applied successfully");