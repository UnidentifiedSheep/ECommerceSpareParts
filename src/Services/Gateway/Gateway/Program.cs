using System.Text;
using Core.StaticFunctions;
using Gateway.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);
builder.Configuration.AddJsonFromDirectory("ReverseProxy");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var iss = builder.Configuration["JwtBearer:ValidIssuer"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = iss,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearer:IssuerSigningKey"]!)),
    };
});

var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.CopyRequestHeaders = true;
        builderContext.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Add("X-Gateway-Token", secret);
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
app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapMethods("{**any}", ["OPTIONS"], () => Results.Ok())
    .AllowAnonymous();

app.MapReverseProxy();

app.Run();