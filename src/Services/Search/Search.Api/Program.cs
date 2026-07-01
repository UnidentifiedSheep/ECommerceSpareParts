using System.Reflection;
using Api.Common;
using Api.Common.Extensions;
using Carter;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using MassTransit;
using RabbitMq.Extensions;
using Search.Abstractions.Options;
using Search.Application;
using Search.Application.Consumers.Producer;
using Search.Application.Consumers.Product;
using Search.Persistence;
using Security;

var builder = WebApplication.CreateBuilder(args);
var env = builder.AddServiceConfiguration("search");

builder.Host.AddLokiLogger(
    builder.Configuration,
    "search.api",
    env);

AddOpenSearchOptions(builder.Services)
    .AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions();

builder.Services.AddCommonApiInfrastructure();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(ProductUpdatedConsumer)));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(context);

        cfg.ReceiveEndpoint(
            "search-queue",
            ep =>
            {
                ep.Durable = true;

                ep.ConfigureConsumer<ProductUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductDeletedConsumer>(context);
                ep.ConfigureConsumer<ProductSizesUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductWeightUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProductLinkageUpdatedConsumer>(context);
                ep.ConfigureConsumer<ProducerUpdatedConsumer>(context);
            });
    });
});

builder.Services
    .AddEComAuth(builder.Configuration)
    .AddMinimalSecurityLayer()
    .AddIntegrationClients()
    .AddApplicationLayer(builder.Configuration)
    .AddPersistenceLayer()
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
return;

static IServiceCollection AddOpenSearchOptions(IServiceCollection services)
{
    services.AddOptions<OpenSearchOptions>()
        .BindConfiguration(OpenSearchOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();
    return services;
}