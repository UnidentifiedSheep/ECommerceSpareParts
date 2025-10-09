using System.Net.Http.Headers;
using Amazon.Runtime;
using Amazon.S3;
using Core.Interfaces;
using Core.Interfaces.Integrations;
using Integrations.Interfaces;
using Integrations.Models.ExchangeRates;
using Integrations.S3;
using Integrations.TimeWebCloud;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integrations;

public static class ServiceProvider
{
    public static IServiceCollection AddIntegrations(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddHttpClient();
        collection.AddHttpClient("TimewebClient", sp =>
        {
            sp.BaseAddress = new Uri(configuration.GetValue<string>("TimeWebConnect:BaseUrl")!);
            sp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                configuration.GetValue<string>("TimeWebConnect:Token")!);
        });

        collection.Configure<ExchangeRatesOptions>(configuration.GetSection("Exchange"));

        collection.AddScoped<ITimeWebMail, TimeWebMail>();
        collection.AddScoped<IExchangeRates, ExchangeRates.ExchangeRates>();

        var awsOptions = configuration.GetAWSOptions("S3Storage");
        awsOptions.Credentials = new BasicAWSCredentials(configuration["S3Storage:AccessKey"],
            configuration["S3Storage:SecretKey"]);
        collection.AddDefaultAWSOptions(awsOptions);


        collection.AddScoped<IS3StorageService, S3StorageService>();
        collection.AddAWSService<IAmazonS3>();

        return collection;
    }
}