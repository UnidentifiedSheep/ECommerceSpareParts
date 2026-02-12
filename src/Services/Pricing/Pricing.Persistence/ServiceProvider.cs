using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using BulkValidation.Pgsql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DbValidator;
using Persistence.Services.UnitOfWork;
using Pricing.Abstractions.Interfaces.DbRepositories;
using Pricing.Persistence.Contexts;
using Pricing.Persistence.Repositories;

namespace Pricing.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddDbContext<DContext>(options => options.UseNpgsql(connectionString));
        
        collection.AddScoped<IMarkupRepository, MarkupRepository>();
        collection.AddScoped<ISettingsRepository, SettingsRepository>();
        
        collection.AddScoped<IUnitOfWork, UnitOfWork<DContext>>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();
        
        return collection;
    }
}