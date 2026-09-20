using Abstractions.Interfaces;
using Abstractions.Models.Options;
using Api.Common.Middleware;
using Api.Common.OperationFilters;
using Application.Common.Diagnostics;
using Application.Common.Models;
using Cache;
using Common;
using Locan.AspNetCore;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Persistence;

namespace Api.Common.Extensions;

public static class WebApplicationBuilderExtensions
{
	private static readonly string[] HealthCheckTags = ["dependency"];
	private static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(5);

	public static string AddServiceConfiguration(
		this IHostApplicationBuilder builder,
		string serviceName,
		string configsPath = "/app/configs")
	{
		var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

		builder
			.Configuration
			.AddAppSettingsFromJsons(environment)
			.AddAppSettingsFromJsons(environment, configsPath)
			.AddConfigsFromJsons(
				serviceName,
				environment,
				configsPath);

		return environment;
	}

	public static IServiceCollection AddCommonApiInfrastructure(
		this IServiceCollection services,
		IServiceDefinition serviceDefinition)
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
		services.AddHealthChecks();
		services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy =>
			{
				policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
			});
		});
		services.AddOpenTelemetry(
			serviceDefinition,
			"api",
			true,
			true);

		services.AddTransient<HeaderSecretMiddleware>();

		services.AddLocanAspNetCore(options =>
		{
			options.DefaultCulture = "en";
			options.SupportedCultures = ["en", "ru", "tr"];
		});

		return services;
	}

	public static IServiceCollection AddPostgresHealthCheck(this IServiceCollection services)
	{
		services
			.AddHealthChecks()
			.AddNpgSql(
				sp => sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString,
				name: "postgres",
				tags: HealthCheckTags,
				timeout: HealthCheckTimeout);

		return services;
	}

	public static IServiceCollection AddRedisHealthCheck(this IServiceCollection services)
	{
		services
			.AddHealthChecks()
			.AddRedis(
				sp => sp.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString,
				name: "redis",
				tags: HealthCheckTags,
				timeout: HealthCheckTimeout);

		return services;
	}

	public static IServiceCollection AddCommonWorkerInfrastructure(
		this IServiceCollection services,
		IServiceDefinition serviceDefinition)
	{
		services.AddProjectJsonSerialization();
		services.AddOpenTelemetry(serviceDefinition, "worker");

		services.AddLocanAspNetCore(options =>
		{
			options.DefaultCulture = "en";
			options.SupportedCultures = ["en", "ru", "tr"];
		});

		return services;
	}

	private static IServiceCollection AddOpenTelemetry(
		this IServiceCollection collection,
		IServiceDefinition serviceDefinition,
		string serviceNameSuffix,
		bool includeAspNetCoreInstrumentation = false,
		bool includePrometheusMetrics = false)
	{
		var openTelemetry = collection
			.AddOpenTelemetry()
			.ConfigureResource(x => x.AddService($"{serviceDefinition.ServiceName}.{serviceNameSuffix}"))
			.WithTracing(tracing =>
			{
				tracing
					.AddSource(CqrsDiagnostics.ActivitySourceName)
					.AddNpgsql()
					.AddHttpClientInstrumentation()
					.AddOtlpExporter();

				if (includeAspNetCoreInstrumentation)
					tracing.AddAspNetCoreInstrumentation();
			});

		if (includePrometheusMetrics)
			openTelemetry.WithMetrics(metrics =>
			{
				metrics
					.AddAspNetCoreInstrumentation()
					.AddProcessInstrumentation()
					.AddRuntimeInstrumentation()
					.AddPrometheusExporter();
				metrics.AddMeter(CqrsMetrics.MeterName);
			});

		return collection;
	}
}
