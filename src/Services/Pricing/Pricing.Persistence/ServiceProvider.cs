using Abstractions.Interfaces;
using BulkValidation.Pgsql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence;
using Persistence.DbValidator;
using Persistence.Extensions;
using Pricing.Persistence.Contexts;

namespace Pricing.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection)
    {
        collection.AddDbContext<DContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(dbOptions.ConnectionString);
        });

        collection.AddUnitOfWork<DContext>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();

        return collection;
    }
}