using Core.Behavior;
using Core.Redis;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonoliteUnicorn.EndPoints.Articles.CreateArticle;
using MonoliteUnicorn.HangFireTasks;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Balances;
using MonoliteUnicorn.Services.Inventory;
using MonoliteUnicorn.Services.Prices.Price;
using MonoliteUnicorn.Services.Prices.PriceGenerator;
using MonoliteUnicorn.Services.Purchase;
using MonoliteUnicorn.Services.Sale;
using Testcontainers.PostgreSql;
using Purchase = MonoliteUnicorn.PostGres.Main.Purchase;
using Sale = MonoliteUnicorn.PostGres.Main.Sale;

namespace Tests;

public static class ServiceProviderForTests
{
    public static ServiceProvider Build(string postgresConnectionString, string redisConnectionString)
    {
        var services = new ServiceCollection();
        
        var endpointAssembly = typeof(CreateArticleCommand).Assembly;
        
        services.AddDbContext<DContext>(options => 
            options.UseNpgsql(postgresConnectionString)
                .EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information));
        
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(endpointAssembly);
        });
        
        services.AddValidatorsFromAssembly(endpointAssembly);
        services.AddScoped<UpdateCurrencyRate>();
        services.AddScoped<MarkUpGenerator>();
        services.AddScoped<UpdateMarkUp>();
        services.AddScoped<IPrice, Price>();
        services.AddScoped<IPurchase, MonoliteUnicorn.Services.Purchase.Purchase>();
        services.AddScoped<ISale, MonoliteUnicorn.Services.Sale.Sale>();
        services.AddScoped<IBalance, Balance>();
        services.AddScoped<IInventory, Inventory>();
        services.AddScoped<IPurchaseOrchestrator, PurchaseOrchestrator>();
        services.AddScoped<ISaleOrchestrator, SaleOrchestrator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}