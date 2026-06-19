using Api.Common.ExceptionHandlers;
using Api.Common.Models.Options;
using Api.Common.Services;
using Application.Common.Interfaces;
using Application.Common.Models.Options;
using Cache;
using Persistence;
using RabbitMq;
using S3;

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

    public static IServiceCollection AddMessageBrokerOptions(this IServiceCollection collection)
    {
        collection.AddOptions<MessageBrokerOptions>()
            .BindConfiguration(MessageBrokerOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return collection;
    }

    public static IServiceCollection AddHeaderSecretsOptions(this IServiceCollection collection)
    {
        collection.AddOptions<HeaderSecretOptions>()
            .BindConfiguration(HeaderSecretOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return collection;
    }

    public static IServiceCollection AddRedisOptions(this IServiceCollection collection)
    {
        collection.AddOptions<RedisOptions>()
            .BindConfiguration(RedisOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return collection;
    }

    public static IServiceCollection AddDatabaseOptions(this IServiceCollection collection)
    {
        collection.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return collection;
    }

    public static IServiceCollection AddS3Options(this IServiceCollection collection)
    {
        collection.AddOptions<S3Options>()
            .BindConfiguration(S3Options.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return collection;
    }

    public static IServiceCollection AddLrtOptions(this IServiceCollection collection)
    {
        collection.AddOptions<LrtExecutorOptions>()
            .BindConfiguration(LrtExecutorOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        return collection;
    }

    public static IServiceCollection AddSystemOptions(this IServiceCollection collection)
    {
        collection.AddOptions<SystemOptions>()
            .BindConfiguration(SystemOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        return collection;
            
    }
}