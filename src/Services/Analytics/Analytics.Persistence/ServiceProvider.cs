using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Persistence.Context;
using Analytics.Persistence.DataSeeds;
using Analytics.Persistence.Repositories;
using BulkValidation.Pgsql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DbValidator;
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
        collection.AddScoped<IPurchaseFactRepository, PurchaseFactRepository>();
        collection.AddScoped<ISalesRepository, SalesRepository>();
        
        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();

        collection.AddScoped<ISeed<DContext>, CurrencySeed>();

        return collection;
    }
}