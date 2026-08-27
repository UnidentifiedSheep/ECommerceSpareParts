using System.Reflection;
using Abstractions;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Api.Common.HostedServices.Startup;
using Application.Common.Interfaces;
using Carter;
using Internal.Integration.Di;
using Localization.Domain.Extensions;
using MassTransit;
using OpenTelemetry.Metrics;
using RabbitMq.Extensions;
using Search.Abstractions.Options;
using Search.Api.GraphQl;
using Search.Application;
using Search.Application.Consumers.CatalogueCandidate;
using Search.Application.Consumers.Producer;
using Search.Application.Consumers.Product;
using Search.Persistence;
using Security;

const string serviceName = "Search";

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

builder.Services.AddCommonApiInfrastructure(ServicesDefinitions.Search)
    .AddGraphQlServices(serviceName);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProducerUpdatedConsumer, ProducerUpdatedConsumerDefinition>();
    x.AddConsumer<ProductUpdatedConsumer, ProductUpdatedConsumerDefinition>();
    x.AddConsumer<ProductDeletedConsumer, ProductDeletedConsumerDefinition>();
    x.AddConsumer<CatalogueCandidateUpdatedConsumer, CatalogueCandidateUpdatedConsumerDefinition>();
    x.AddConsumer<CatalogueCandidateDeletedConsumer, CatalogueCandidateDeletedConsumerDefinition>();

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
                ep.ConfigureConsumer<CatalogueCandidateUpdatedConsumer>(context);
                ep.ConfigureConsumer<CatalogueCandidateDeletedConsumer>(context);

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

builder.Services.AddScoped<IStartupTask, LoadLocalesStartupTask>();
builder.Services.AddHostedService<StartupTaskHostedService>();

var app = builder.Build();

app.UseCommonApiPipeline();
app.MapGraphQL();

await app.RunWithGraphQLCommandsAsync(args);
return;

static IServiceCollection AddOpenSearchOptions(IServiceCollection services)
{
    services.AddOptions<OpenSearchOptions>()
        .BindConfiguration(OpenSearchOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();
    return services;
}
