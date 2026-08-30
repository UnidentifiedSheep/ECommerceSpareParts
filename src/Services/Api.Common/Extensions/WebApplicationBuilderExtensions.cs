using Abstractions.Interfaces;
using Abstractions.Models.Options;
using Api.Common.Middleware;
using Api.Common.OperationFilters;
using Application.Common.Diagnostics;
using Application.Common.Models;
using Common;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Common.Extensions;

public static class WebApplicationBuilderExtensions
{
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
		services.AddConfiguredRequestLocalization();
		services.AddBaseExceptionHandlers();
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

		return services;
	}

	public static IServiceCollection AddCommonWorkerInfrastructure(
		this IServiceCollection services,
		IServiceDefinition serviceDefinition)
	{
		services.AddProjectJsonSerialization();
		services.AddOpenTelemetry(serviceDefinition, "worker");

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
