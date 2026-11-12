using Persistence.Context;
using Security.Core.Interfaces;
using Abstractions.Models;
using Api.Common;
using Application.Common.Interfaces.Validators;
using Common;
using Main.Migrator;
using Main.Migrator.DataSeeds;
using Main.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Persistence;
using Persistence.Extensions;
using Persistence.Interfaces;
using Security.Models;
using Security.Services;

var builder = Host
	.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((_, config) =>
		config
			.AddMigratorSettingsFromJsons("main.settings")
			.AddMigratorSettingsFromJsons("main.settings", "/app/configs"));

var seedingRequested = false;

builder.ConfigureServices((context, services) =>
{
	services.AddDatabaseOptions();

	var seedValue = context.Configuration.GetValue<bool?>("Seed");
	seedingRequested = seedValue == true;

	//add db context
	services.AddDbContextModule<MainDbContextModule>();
	services.AddDbContext<UnifiedDbContext>((sp, options) =>
	{
		var connectionString = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString;
		options.UseNpgsql(connectionString, x => x.MigrationsAssembly("Main.Migrator"));
	});

	//used for password hash etc
	services.AddSingleton(new Hasher());
	services.AddSingleton<IPasswordHasher>(sp => sp.GetRequiredService<Hasher>());
	services.AddSingleton<IValueHasher>(sp => sp.GetRequiredService<Hasher>());
	services.AddSingleton(new PasswordRules());
	services.AddSingleton<IPasswordManager, PasswordManager>();

	services
		.AddOptions<ServiceSecrets>()
		.BindConfiguration(ServiceSecrets.SectionName)
		.ValidateDataAnnotations()
		.ValidateOnStart();
});

builder.ConfigureServices((_, services) =>
{
	services.AddScoped<ISeed<UnifiedDbContext>, PermissionSeed>();
	services.AddScoped<ISeed<UnifiedDbContext>, RoleSeed>();
	services.AddScoped<ISeed<UnifiedDbContext>, RolePermissionSeed>();
	services.AddScoped<ISeed<UnifiedDbContext>, UserSeed>();
	services.AddScoped<ISeed<UnifiedDbContext>, SystemOrganizationSeed>();
	services.AddScoped<ISeed<UnifiedDbContext>, CurrencySeed>();
	services.AddScoped<ISeed<UnifiedDbContext>, AdminSeed>();
	services.AddScoped<ISeed<UnifiedDbContext>, ScheduledLrtSeed>();
});

var host = builder.Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<UnifiedDbContext>();

await db.Database.MigrateAsync();

Console.WriteLine("Main migrations applied successfully");

if (!seedingRequested)
	return;

Console.WriteLine("Seeding database...");

await host.SeedAsync<UnifiedDbContext>();

Console.WriteLine("Database seeded successfully");
