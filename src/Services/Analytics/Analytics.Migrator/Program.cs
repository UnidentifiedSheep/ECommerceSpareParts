using Analytics.Persistence.Context;
using Api.Common;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Persistence;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
        config.AddMigratorSettingsFromJsons("analytics.settings")
            .AddMigratorSettingsFromJsons("analytics.settings", "/app/configs"));

builder.ConfigureServices((_, services) =>
{
    services.AddDatabaseOptions();

    services.AddDbContext<DContext>((sp, options) =>
    {
        var connectionString = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString;
        options.UseNpgsql(connectionString, x => x.MigrationsAssembly("Analytics.Migrator"));
    });
});

var host = builder.Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<DContext>();

await db.Database.MigrateAsync();

Console.WriteLine("Analytics migrations applied successfully");