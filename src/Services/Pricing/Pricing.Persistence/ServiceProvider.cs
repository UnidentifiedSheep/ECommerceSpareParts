using Abstractions.Interfaces;
using Application.Common.Interfaces.Repositories;
using BulkValidation.Pgsql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence;
using Persistence.DbValidator;
using Persistence.Extensions;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Persistence.Contexts;
using Pricing.Persistence.Repositories;

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

        collection.AddScoped(typeof(IRepository<,>), typeof(BasicEfRepository<,>));
        collection.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));
        
        collection.AddScoped<IPriceOfferRepository, PriceOfferRepository>();
        collection.AddScoped<IProductPriceOptionRepository, ProductPriceOptionRepository>();
        collection.AddScoped<IPriceOfferRefreshStateRepository, PriceOfferRefreshStateRepository>();
        collection.AddScoped<IPriceRecalculationRequestRepository, PriceRecalculationRequestRepository>();

        collection.AddUnitOfWork<DContext>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();

        return collection;
    }
}