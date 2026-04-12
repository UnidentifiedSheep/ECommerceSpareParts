using Abstractions.Interfaces;
using BulkValidation.Pgsql.Extensions;
using Main.Application.Interfaces.Repositories;
using Main.Persistence.Context;
using Main.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DbValidator;
using Persistence.Extensions;

namespace Main.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddDbContext<DContext>(options => options.UseNpgsql(connectionString));
        
        collection.AddScoped<IProductRepository, ProductRepository>();
        collection.AddScoped<IProductWeightRepository, ProductWeightRepository>();
        collection.AddScoped<IReadDContext, ReadDContext>();
        
        collection.AddUnitOfWork<DContext>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();


        return collection;
    }
}