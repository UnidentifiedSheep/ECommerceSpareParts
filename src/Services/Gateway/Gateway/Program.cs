using System.Text.Json.Nodes;
using Abstractions;
using Api.Common;
using Api.Common.Extensions;
using Application.Common.Backplane;
using Cache;
using Common;
using Gateway.Application;
using Gateway.EndPoints;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using Localization.Domain.Middlewares;
using MassTransit;
using OpenTelemetry.Metrics;
using RabbitMq.Extensions;
using Scalar.AspNetCore;
using Yarp.ReverseProxy.Transforms;
using ZiggyCreatures.Caching.Fusion.Backplane;

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

builder.Services
    .AddBaseExceptionHandlers()
    .AddCacheLayer("gateway")
    .AddLocalization(builder.Configuration)
    .AddApplicationLayer(builder.Configuration)
    .AddIntegrationClients()
    .AddReverseProxy()
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

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
        else
            policy.SetIsOriginAllowed(_ => true);
    });
});

var uniqQueueName = $"queue-of-gateway-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BackplaneConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(context);

        cfg.ReceiveEndpoint(uniqQueueName, ep =>
        {
            ep.AutoDelete = true;
            ep.Durable = false;

            ep.ConfigureConsumeTopology = false;
            
            ep.ConfigureConsumer<BackplaneConsumer>(context);
            ep.Bind<BackplaneMessage>();
        });

        cfg.ReceiveEndpoint("gateway-queue", ep =>
        {
            ep.Durable = true;
        });
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

MapDocs(app);

app.UseCors();

app.UseAuthorization();

app.UseWebSockets();
app.MapJobEndPoints();
app.MapReverseProxy();

app.UseMiddleware<ScopedLocalizationMiddleware>();

app.UseExceptionHandler(_ => { });

app.UseOpenTelemetryPrometheusScrapingEndpoint();
await app.RunAsync();


void MapDocs(WebApplication application)
{
    application.MapGet("/docs/openapi/{service}.json", async (
        string service,
        HttpContext context,
        IHttpClientFactory httpClientFactory) =>
    {
        var services = new Dictionary<string, string>
        {
            [ServicesDefinitions.Main.ServiceName] = "/main/swagger/v1/swagger.json",
            [ServicesDefinitions.Analytics.ServiceName] = "/analytics/swagger/v1/swagger.json",
            [ServicesDefinitions.Search.ServiceName] = "/search/swagger/v1/swagger.json",
            [ServicesDefinitions.Pricing.ServiceName] = "/pricing/swagger/v1/swagger.json"
        };

        if (!services.TryGetValue(service, out var swaggerPath))
            return Results.NotFound();

        var request = context.Request;

        var baseUrl = $"{request.Scheme}://{request.Host}";

        var client = httpClientFactory.CreateClient();

        var swaggerUrl = $"{baseUrl}{swaggerPath}";

        var json = await client.GetStringAsync(swaggerUrl);

        var node = JsonNode.Parse(json);

        if (node is null)
            return Results.Problem("Invalid OpenAPI document.");

        node["servers"] = new JsonArray
        {
            new JsonObject
            {
                ["url"] = $"/{service}"
            }
        };

        return Results.Content(
            node.ToJsonString(),
            "application/json");
    });

    application.MapScalarApiReference("/docs", options =>
    {
        options
            .AddDocument(
                ServicesDefinitions.Main.ServiceName, 
                "Main API", 
                "/docs/openapi/main.json")
            .AddDocument(
                ServicesDefinitions.Analytics.ServiceName, 
                "Analytics API", 
                "/docs/openapi/analytics.json")
            .AddDocument(
                ServicesDefinitions.Search.ServiceName, 
                "Search API", 
                "/docs/openapi/search.json")
            .AddDocument(
                ServicesDefinitions.Pricing.ServiceName, 
                "Pricing API", 
                "/docs/openapi/pricing.json");
    });
}
