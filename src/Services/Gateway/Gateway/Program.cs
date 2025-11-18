using System.Text;
using Core.StaticFunctions;
using Gateway.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
Certs.RegisterCerts("/app/certs");
builder.Configuration.AddJsonFromDirectory("ReverseProxy");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AMW", policy => { policy.RequireRole("ADMIN", "MODERATOR", "WORKER"); });
    options.AddPolicy("AM", policy => { policy.RequireRole("ADMIN", "MODERATOR"); });
    options.AddPolicy("MEMBER", policy => { policy.RequireRole("MEMBER"); });
});

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

var secret = builder.Configuration["Gateway:Secret"];

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