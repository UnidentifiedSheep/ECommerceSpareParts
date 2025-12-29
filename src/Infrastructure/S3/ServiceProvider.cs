using Amazon.S3;
using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace S3;

public static class ServiceProvider
{
    public static IServiceCollection AddS3(this IServiceCollection collection, Func<AmazonS3Client> getOptions)
    {
        collection.AddSingleton<IAmazonS3>(getOptions());
        collection.AddScoped<IS3StorageService, S3StorageService>();
        return collection;
    }
}