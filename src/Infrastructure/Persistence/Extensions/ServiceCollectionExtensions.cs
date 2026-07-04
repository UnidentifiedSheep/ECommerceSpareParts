using Abstractions.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interfaces;
using Persistence.Services;
using Persistence.Services.UnitOfWork;

namespace Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWork<T>(this IServiceCollection collection) where T : DbContext
    {
        collection.AddScoped<IUnitOfWork, UnitOfWork<T>>();
        collection.AddScoped<IContextMetadata, ContextMetadata<T>>();
        collection.AddScoped<IQueryableExtensions, QueryableExtensions>();

        return collection;
    }
}