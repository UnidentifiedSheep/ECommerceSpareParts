using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddCommandLine(args);
    });

builder.ConfigureServices((context, services) =>
{
    var connectionString = context.Configuration["ConnectionString"];
    services.AddDbContext<DContext>(
        options => options.UseNpgsql(connectionString, 
            x => x.MigrationsAssembly("Main.Migrator")));
});

var host = builder.Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<DContext>();

await db.Database.MigrateAsync();

Console.WriteLine("Main migrations applied successfully");