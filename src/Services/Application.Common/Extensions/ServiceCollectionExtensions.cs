using Abstractions.Interfaces.RelatedData;
using Application.Common.Abstractions.RelatedData;
using Application.Common.Interfaces;
using Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterRelatedData(this IServiceCollection collection)
    {
        collection.AddScoped<IRelatedDataFactory, RelatedDataFactory>();
        collection.AddScoped<IRelatedDataCollector, RelatedDataCollector>();

        return collection;
    }
}