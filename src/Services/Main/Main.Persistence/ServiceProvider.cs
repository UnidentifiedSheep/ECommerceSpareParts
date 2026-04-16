using Abstractions.Interfaces;
using Application.Common.Interfaces.Repositories;
using BulkValidation.Pgsql.Extensions;
using Main.Application.Interfaces.Repositories;
using Main.Persistence.Context;
using Main.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DbValidator;
using Persistence.Extensions;
using Persistence.Interceptors;
using ProducerRepository = Main.Persistence.Repositories.Producer.ProducerRepository;

namespace Main.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddScoped<AuditableEntitySaveChangesInterceptor>();
        collection.AddDbContext<DContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });
        
        collection.AddScoped<IProductRepository, ProductRepository>();
        collection.AddScoped<IProducerRepository, ProducerRepository>();
        collection.AddScoped(typeof(IRepository<,>), typeof(BasicEfRepository<,>));
        collection.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));
        
        collection.AddUnitOfWork<DContext>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();


        return collection;
    }
}