using Abstractions.Interfaces.Services;
using Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Services.UnitOfWork;

namespace Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWork<T>(this IServiceCollection collection) where T : DbContext
    {
        collection.AddScoped<IUnitOfWork, UnitOfWork<T>>();

        return collection;
    }
}