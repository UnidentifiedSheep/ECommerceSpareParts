using Analytics.Core.Interfaces.DbRepositories;
using Analytics.Persistence.Context;
using Analytics.Persistence.Repositories;
using Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddDbContext<DContext>(options => options.UseNpgsql(connectionString));
        
        collection.AddScoped<IUnitOfWork, UnitOfWork>();

        collection.AddScoped<ICurrencyRepository, CurrencyRepository>();
        collection.AddScoped<ISellInfoRepository, SellInfoRepository>();

        return collection;
    }
}