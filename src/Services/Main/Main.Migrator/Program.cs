using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Api.Common;
using Common;
using Main.Migrator;
using Main.Migrator.DataSeeds;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Persistence;
using Persistence.Extensions;
using Persistence.Interfaces;
using Security.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
        config.AddMigratorSettingsFromJsons("main.settings")
            .AddMigratorSettingsFromJsons("main.settings", "/app/configs"));


var seedingRequested = false;

builder.ConfigureServices((context, services) =>
{
    services.AddDatabaseOptions();

    var seedValue = context.Configuration.GetValue<bool?>("Seed");
    seedingRequested = seedValue == true;

    //add db context
    services.AddDbContext<DContext>((sp, options) =>
    {
        var connectionString = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString;
        options.UseNpgsql(connectionString, x => x.MigrationsAssembly("Main.Migrator"));
    });

    //used for password hash etc
    services.AddSingleton<IPasswordManager, PasswordManager>(_ => new PasswordManager(new PasswordRules()));

    services.AddOptions<ServiceSecrets>()
        .BindConfiguration(ServiceSecrets.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();
});

builder.ConfigureServices((_, services) =>
{
    services.AddScoped<ISeed<DContext>, PermissionSeed>();
    services.AddScoped<ISeed<DContext>, RoleSeed>();
    services.AddScoped<ISeed<DContext>, RolePermissionSeed>();
    services.AddScoped<ISeed<DContext>, UserSeed>();
    services.AddScoped<ISeed<DContext>, SystemOrganizationSeed>();
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
