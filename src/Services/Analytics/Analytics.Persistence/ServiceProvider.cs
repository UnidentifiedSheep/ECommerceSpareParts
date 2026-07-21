using Abstractions.Interfaces;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Persistence.Context;
using Analytics.Persistence.Repositories;
using Application.Common.Interfaces.Repositories;
using BulkValidation.Pgsql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence;
using Persistence.Common;
using Persistence.DbValidator;
using Persistence.Extensions;
using Persistence.Interceptors;

namespace Analytics.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection)
    {
        collection.AddScoped<AuditableEntitySaveChangesInterceptor>();
        collection.AddScoped<DomainEventFlusherSaveChangesInterceptor>();
        collection.AddDbContext<DContext>((sp, options) =>
        {
            var op = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(op.ConnectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
            options.AddInterceptors(sp.GetRequiredService<DomainEventFlusherSaveChangesInterceptor>());
        });

        collection.AddUnitOfWork<DContext>();

        collection.AddScoped(typeof(IRepository<,>), typeof(BasicEfRepository<,>));
        collection.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));
        collection.AddJobRepositories<DContext>();

        collection.AddScoped<IMetricRepository, MetricRepository>();
        collection.AddScoped<ISaleFactRepository, SaleFactRepository>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();

        return collection;
    }
}