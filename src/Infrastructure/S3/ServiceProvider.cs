using Amazon.Runtime;
using Amazon.S3;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace S3;

public static class ServiceProvider
{
    public static IServiceCollection AddS3(this IServiceCollection collection, IConfiguration config)
    {
        var awsOptions = config.GetAWSOptions();
        awsOptions.Credentials = new BasicAWSCredentials(config["AWS:AccessKey"], config["AWS:SecretKey"]);
        collection.AddAWSService<IAmazonS3>(awsOptions);
        collection.AddScoped<IS3StorageService, S3StorageService>();
        return collection;
    }
}