using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Main.Migrator.DataSeeds;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Extensions;
using Persistence.Interfaces;
using Security.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddCommandLine(args);
    });

bool seedingRequested = false;

builder.ConfigureServices((context, services) =>
{
    var connectionString = context.Configuration["ConnectionString"];
    
    var seedValue = context.Configuration.GetValue<string?>("Seed");
    if (seedValue == "true")
        seedingRequested = true;
    
    //add db context
    services.AddDbContext<DContext>(
        options => options.UseNpgsql(connectionString, 
            x => x.MigrationsAssembly("Main.Migrator")));
    
    //used for password hash etc
    services.AddSingleton<IPasswordManager, PasswordManager>(
        _ => new PasswordManager(new PasswordRules()));
});

builder.ConfigureServices((_, services) =>
{
    services.AddScoped<ISeed<DContext>, PermissionSeed>();
    services.AddScoped<ISeed<DContext>, RoleSeed>();
    services.AddScoped<ISeed<DContext>, RolePermissionSeed>();
    services.AddScoped<ISeed<DContext>, UserSeed>();
    services.AddScoped<ISeed<DContext>, CurrencySeed>();
    services.AddScoped<ISeed<DContext>, AdminSeed>();
});

var host = builder.Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<DContext>();

await db.Database.MigrateAsync();

Console.WriteLine("Main migrations applied successfully");

if (!seedingRequested) return;

Console.WriteLine("Seeding database...");

await host.SeedAsync<DContext>();

Console.WriteLine("Database seeded successfully");