using Abstractions.Interfaces;
using BulkValidation.Pgsql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DbValidator;
using Persistence.Extensions;
using Pricing.Persistence.Contexts;

namespace Pricing.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddDbContext<DContext>(options => options.UseNpgsql(connectionString));

        collection.AddUnitOfWork<DContext>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();

        return collection;
    }
}