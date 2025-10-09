using Api.Common.Services;
using Core.Interfaces;

namespace Api.Common;

public static class ServiceProvider
{
    public static IServiceCollection AddCommonLayer(this IServiceCollection collection)
    {
        collection.AddSingleton<ISearchLogger, SearchLogger>();
        return collection;
    }
}