using Api.Common.ExceptionHandlers;
using Api.Common.Services;
using Application.Common.Interfaces;

namespace Api.Common;

public static class ServiceProvider
{
    public static IServiceCollection AddCommonLayer(this IServiceCollection collection)
    {
        collection.AddSingleton<ISearchLogger, SearchLogger>();
        return collection;
    }

    public static IServiceCollection AddBaseExceptionHandlers(this IServiceCollection collection)
    {
        collection.AddExceptionHandler<ValidationExceptionHandler>();
        collection.AddExceptionHandler<DbValidationExceptionHandler>();
        collection.AddExceptionHandler<AnyExceptionHandler>();
        return collection;
    }
}