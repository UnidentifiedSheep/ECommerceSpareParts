using Api.Common.ExceptionHandlers;
using Api.Common.Middleware;
using Search.Api.Contexts;
using Search.Api.EndPoints;
using Search.Application;
using Search.Persistence;
using Security.Utils;

var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddExceptionHandler<AotExceptionHandler>();

builder.Services.AddPersistenceLayer(Environment.GetEnvironmentVariable("INDEX_FOLDER") ?? "./data")
    .AddApplicationLayer();

var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY")!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.UseMiddleware<HeaderSecretMiddleware>();

app.UseExceptionHandler(_ => { });

app.MapSuggestionEndpoints()
    .MapDataEndpoints();

app.Run();

