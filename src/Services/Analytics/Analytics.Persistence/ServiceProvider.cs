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
using Persistence.DbValidator;
using Persistence.Extensions;

namespace Analytics.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection)
    {
        collection.AddDbContext<DContext>((sp, options) =>
        {
            var op = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(op.ConnectionString);
        });

        collection.AddUnitOfWork<DContext>();

        collection.AddScoped(typeof(IRepository<,>), typeof(BasicEfRepository<,>));
        collection.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));

        collection.AddScoped<IMetricRepository, MetricRepository>();
        collection.AddScoped<ISalesFactRepository, SalesFactRepository>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();

        return collection;
    }
}
