using Api.Common;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Persistence;
using Persistence.Context;
using Persistence.Extensions;
using Persistence.Interfaces;
using Pricing.Migrator.Seeds;
using Pricing.Persistence;

var builder = Host
	.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((_, config) =>
		config
			.AddMigratorSettingsFromJsons("pricing.settings")
			.AddMigratorSettingsFromJsons("pricing.settings", "/app/configs"));

builder.ConfigureServices((_, services) =>
{
	services.AddDatabaseOptions();
	services.AddDbContextModule<PricingDbContextModule>();

	services.AddDbContext<UnifiedDbContext>((sp, options) =>
	{
		var connectionString = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString;
		options.UseNpgsql(connectionString, x => x.MigrationsAssembly("Pricing.Migrator"));
	});
});

builder.ConfigureServices((_, services) =>
{
	services.AddScoped<ISeed<UnifiedDbContext>, MarkupGroupSeed>();
});

var host = builder.Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<UnifiedDbContext>();

await db.Database.MigrateAsync();

Console.WriteLine("Pricing migrations applied successfully");

Console.WriteLine("Seeding database...");

await host.SeedAsync<UnifiedDbContext>();

Console.WriteLine("Database seeded successfully");
