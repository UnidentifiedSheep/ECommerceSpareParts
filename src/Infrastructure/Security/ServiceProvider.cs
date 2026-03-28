using System.Text.Json;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Security.Models;
using Security.Services;

namespace Security;

public static class ServiceProvider
{
    public static IServiceCollection AddSecurityLayer(
        this IServiceCollection collection,
        PasswordRules? passwordRules = null)
    {
        collection.AddSingleton<IJwtGenerator, JwtGenerator>();
        collection.AddSingleton<IPasswordManager, PasswordManager>();
        collection.AddSingleton(passwordRules ?? new PasswordRules());
        collection.AddSingleton<ITokenHasher, TokenHasher>();
        collection.AddScoped<IUserContext, UserContext>();

        return collection;
    }

    public static IServiceCollection AddSecurityLayer(
        this IServiceCollection collection,
        string secret,
        JsonSerializerOptions? options = null,
        PasswordRules? passwordRules = null)
    {
        collection.AddSingleton<IJsonSigner, JsonSigner>(_ => new JsonSigner(secret, options));
        collection.AddSecurityLayer(passwordRules);
        return collection;
    }

    public static IServiceCollection AddJwtOptions(this IServiceCollection collection, IConfiguration configuration)
    {
        var issuerSignKey = configuration["JwtBearer:IssuerSigningKey"];
        var validIssuer = configuration["JwtBearer:ValidIssuer"];
        var validDurationMs = configuration["JwtBearer:ValidDurationMs"];
        if (issuerSignKey == null) throw new ArgumentNullException(nameof(issuerSignKey));
        if (validIssuer == null) throw new ArgumentNullException(nameof(validIssuer));
        if (validDurationMs == null) throw new ArgumentNullException(nameof(validDurationMs));

        collection.AddSingleton(new JwtOptions(
            issuerSignKey, 
            validIssuer, 
            TimeSpan.FromMilliseconds(long.Parse(validDurationMs))));
        return collection;
    }
}