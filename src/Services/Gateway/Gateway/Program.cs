using Api.Common.Extensions;
using Common;
using OpenTelemetry.Metrics;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);


var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

builder.Configuration
    .AddAppSettingsFromJsons(env)
    .AddAppSettingsFromJsons(env, "/app/configs")
    .AddConfigsFromJsons("gateway", null, "/app/configs")
    .AddConfigsFromJsons("gateway", env, "/app/configs");

builder.Host.AddLokiLogger(
    builder.Configuration,
    "gateway",
    env);

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DenyAll", policy => { policy.RequireAssertion(_ => false); });
});

var secret = builder.Configuration["HeaderSecret:Key"];

if (secret == null)
    throw new ArgumentNullException(nameof(secret), "HeaderSecret:Key cannot be null.");

var reverseProxySection = builder.Configuration.GetSection("ReverseProxy");
var routeCount = reverseProxySection.GetSection("Routes").GetChildren().Count();
var clusterCount = reverseProxySection.GetSection("Clusters").GetChildren().Count();

Console.WriteLine($"Gateway reverse proxy config loaded. Routes: {routeCount}, clusters: {clusterCount}");

if (routeCount == 0 || clusterCount == 0)
    throw new InvalidOperationException("Gateway reverse proxy config is empty.");

builder.Services.AddReverseProxy()
    .LoadFromConfig(reverseProxySection)
    .AddTransforms(builderContext =>
    {
        builderContext.CopyRequestHeaders = true;

        builderContext.AddRequestTransform(transformContext =>
        {
            var headers = transformContext.ProxyRequest.Headers;
            headers.Remove("X-Internal-Token");
            headers.Add("X-Internal-Token", secret);
            return ValueTask.CompletedTask;
        });
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();


app.UseCors();

app.UseAuthorization();

app.MapReverseProxy();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();
