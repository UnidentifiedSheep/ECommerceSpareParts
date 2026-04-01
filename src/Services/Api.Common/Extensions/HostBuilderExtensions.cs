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
        string environment,
        string? lokiUrl)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Conditional(
                _ => !string.IsNullOrWhiteSpace(lokiUrl),
                wt => wt.LokiHttp(() => new LokiSinkConfiguration
                {
                    LokiUrl = lokiUrl!,
                    LogLabelProvider = new CustomLogLabelProvider([
                        new LokiLabel("service", serviceName),
                        new LokiLabel("env", environment)
                    ])
                })
            )
            .CreateLogger();

        hostBuilder.UseSerilog();
        return  hostBuilder;
    }
}