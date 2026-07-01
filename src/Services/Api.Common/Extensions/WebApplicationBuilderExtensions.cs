using Abstractions.Models.Options;
using Api.Common.Middleware;
using Api.Common.OperationFilters;
using Common;

namespace Api.Common.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static string AddServiceConfiguration(
        this IHostApplicationBuilder builder,
        string serviceName,
        string configsPath = "/app/configs")
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

        builder.Configuration
            .AddAppSettingsFromJsons(environment)
            .AddAppSettingsFromJsons(environment, configsPath)
            .AddConfigsFromJsons(
                serviceName,
                environment,
                configsPath);

        return environment;
    }

    public static IServiceCollection AddCommonApiInfrastructure(
        this IServiceCollection services)
    {
        services.AddProjectJsonSerialization();
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => c.OperationFilter<PermissionsOperationFilter>());
        services.ConfigureHttpJsonOptions(options =>
        {
            ProjectJsonOptions.Configure(options.SerializerOptions);
        });
        services.AddHttpContextAccessor();
        services.AddBaseExceptionHandlers();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        services.AddTransient<HeaderSecretMiddleware>();

        return services;
    }

    public static IServiceCollection AddCommonWorkerInfrastructure(
        this IServiceCollection services)
    {
        services.AddProjectJsonSerialization();

        return services;
    }
}