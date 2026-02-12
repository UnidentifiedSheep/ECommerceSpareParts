using Abstractions.Interfaces.Services;
using Analytics.Core.Interfaces.DbRepositories;
using Analytics.Persistence.Context;
using Analytics.Persistence.DataSeeds;
using Analytics.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interfaces;
using Persistence.Services.UnitOfWork;

namespace Analytics.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddDbContext<DContext>(options => options.UseNpgsql(connectionString));
        
        collection.AddScoped<IUnitOfWork, UnitOfWork<DContext>>();

        collection.AddScoped<ICurrencyRepository, CurrencyRepository>();
        collection.AddScoped<ISellInfoRepository, SellInfoRepository>();

        collection.AddScoped<ISeed<DContext>, CurrencySeed>();

        return collection;
    }
}