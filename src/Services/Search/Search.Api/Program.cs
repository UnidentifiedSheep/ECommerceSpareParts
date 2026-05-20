using System.Reflection;
using Abstractions.Interfaces.HostedServices;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Carter;
using Localization.Domain.Extensions;
using MassTransit;
using RabbitMq.Extensions;
using Search.Application;
using Search.Persistence;
using Security;

var builder = WebApplication.CreateBuilder(args);
var env = builder.AddServiceConfiguration("search");

builder.Host.AddLokiLogger(
    builder.Configuration,
    "search.api",
    env);

builder.Services.AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions();

builder.Services.AddCommonApiInfrastructure();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(context);

        cfg.ReceiveEndpoint("search-queue", ep => { ep.Durable = true; });
    });
});

builder.Services.AddSingleton<BackgroundTaskQueue>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());
builder.Services.AddHostedService<BackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());

builder.Services.AddPersistenceLayer()
    .AddEComAuth(builder.Configuration)
    .AddMinimalSecurityLayer()
    .AddApplicationLayer()
    .AddLocalization(builder.Configuration);

var endpointAssembly = typeof(Program).Assembly;
builder.Services.AddCarter(
    new DependencyContextAssemblyCatalog(endpointAssembly),
    c => c.WithEmptyValidators());

var app = builder.Build();

await app.LoadLocalesFromJson(
    Assembly.GetExecutingAssembly().GetDefaultLocalizationPath());

app.UseCommonApiPipeline();

await app.RunAsync();