using Abstractions.Interfaces.HostedServices;
using Api.Common.ExceptionHandlers;
using Api.Common.HostedServices;
using Api.Common.Middleware;
using MassTransit;
using RabbitMq.Extensions;
using RabbitMq.Models;
using Search.Api.Contexts;
using Search.Api.EndPoints;
using Search.Application;
using Search.Application.Consumers;
using Search.Persistence;
using Security.Utils;

var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);

var builder = WebApplication.CreateSlimBuilder(args);

ConfigureKestrel();

var brokerOptions = new MessageBrokerOptions
{
    Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!,
    Username = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER")!,
    Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")!
};

var uniqQueueName = $"queue-of-search-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ArticledCreatedConsumer>();
    x.AddConsumer<ArticleUpdatedConsumer>();
    x.AddConsumer<ArticleDeletedConsumer>();
    x.AddConsumer<SuggestionRebuildNeededConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(brokerOptions);

        /*cfg.ReceiveEndpoint(uniqQueueName, ep =>
        {
            ep.AutoDelete = true;
            ep.Durable = false;
            ep.ConfigureConsumeTopology = false;
        });*/

        cfg.ReceiveEndpoint("search-queue", ep =>
        {
            ep.Durable = true;

            ep.ConfigureConsumer<ArticledCreatedConsumer>(context);
            ep.ConfigureConsumer<ArticleUpdatedConsumer>(context);
            ep.ConfigureConsumer<ArticleDeletedConsumer>(context);
            ep.ConfigureConsumer<SuggestionRebuildNeededConsumer>(context);
        });
    });
});

builder.Services.AddExceptionHandler<AotExceptionHandler>();

builder.Services.AddSingleton<BackgroundTaskQueue>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());
builder.Services.AddHostedService<BackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());
builder.Services.AddPersistenceLayer(Environment.GetEnvironmentVariable("INDEX_FOLDER") ?? "./data")
    .AddApplicationLayer();

var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY")!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

if (Environment.GetEnvironmentVariable("USE_HTTPS_REDIRECTION") == "true")
    app.UseHttpsRedirection();
        
app.UseMiddleware<HeaderSecretMiddleware>();
app.UseExceptionHandler(_ => { });

app.MapSuggestionEndpoints()
    .MapDataEndpoints();

app.Run();

void ConfigureKestrel()
{
    var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
    var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");
    
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(8080);
        if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
        {
            options.ListenAnyIP(7292, listenOptions =>
            {
                listenOptions.UseHttps(certPath, certPassword);
            });
        }
    });
}