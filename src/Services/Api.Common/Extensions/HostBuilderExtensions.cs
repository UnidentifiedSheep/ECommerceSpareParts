using Api.Common.Logging;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

namespace Api.Common.Extensions;

public static class HostBuilderExtensions
{
	public static IHostBuilder AddLokiLogger(
		this IHostBuilder hostBuilder,
		IConfiguration configuration,
		string serviceName,
		string environment)
	{
		hostBuilder.ConfigureLogging(ConfigureActivityTracking);

		var loggerConfiguration = GetLoggerConfiguration(
			configuration,
			serviceName,
			environment,
			configuration["LokiUrl"]);

		hostBuilder.UseSerilog(loggerConfiguration.CreateLogger());
		return hostBuilder;
	}

	public static IHostApplicationBuilder AddLokiLogger(
		this IHostApplicationBuilder builder,
		IConfiguration configuration,
		string serviceName,
		string environment)
	{
		ConfigureActivityTracking(builder.Logging);

		var logger = GetLoggerConfiguration(
				configuration,
				serviceName,
				environment,
				configuration["LokiUrl"])
			.CreateLogger();
		builder.Logging.AddSerilog(logger);

		return builder;
	}

	private static LoggerConfiguration GetLoggerConfiguration(
		IConfiguration configuration,
		string serviceName,
		string environment,
		string? lokiUrl)
	{
		var loggerConfiguration = new LoggerConfiguration()
			.ReadFrom
			.Configuration(configuration)
			.Enrich
			.FromLogContext();

		if (!string.IsNullOrWhiteSpace(lokiUrl))
			loggerConfiguration.WriteTo.LokiHttp(() => new LokiSinkConfiguration
			{
				LokiUrl = lokiUrl,
				LogLabelProvider = new CustomLogLabelProvider(
					[new LokiLabel("service", serviceName), new LokiLabel("env", environment)])
			});

		return loggerConfiguration;
	}

	private static void ConfigureActivityTracking(ILoggingBuilder logging)
	{
		logging.Configure(options => options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId |
			ActivityTrackingOptions.SpanId | ActivityTrackingOptions.ParentId);
	}
}
