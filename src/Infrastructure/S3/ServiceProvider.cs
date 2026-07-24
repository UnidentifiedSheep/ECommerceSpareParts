using Abstractions.Interfaces;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace S3;

public static class ServiceProvider
{
    public static IServiceCollection AddS3(this IServiceCollection collection)
    {
        collection.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
            var config = CreateConfig(
                options.InternalUrl, 
                options.Region,
                options.ForcePathStyle);
            return new AmazonS3Client(
                options.Login,
                options.Password,
                config);
        });

        collection.AddSingleton<IPresignedS3Client, PresignedS3Client>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
            var config = CreateConfig(
                options.ExternalUrl, 
                options.Region,
                options.ForcePathStyle);
            var protocol = GetProtocol(options.ExternalUrl);
            return new PresignedS3Client(
                new AmazonS3Client(
                    options.Login,
                    options.Password,
                    config),
                protocol);
        });

        collection.AddScoped<IS3StorageService, S3StorageService>();
        return collection;
    }

    private static AmazonS3Config CreateConfig(
        string serviceUrl,
        string region,
        bool forcePathStyle)
    {
        var uri = new Uri(serviceUrl);

        return new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = forcePathStyle,
            AuthenticationRegion = region,
            UseHttp = uri.Scheme == Uri.UriSchemeHttp
        };
    }

    private static Protocol GetProtocol(string serviceUrl)
    {
        var uri = new Uri(serviceUrl);
        return uri.Scheme == Uri.UriSchemeHttp
            ? Protocol.HTTP
            : Protocol.HTTPS;
    }
}