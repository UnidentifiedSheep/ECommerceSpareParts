using System.Text.Json;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Microsoft.Extensions.DependencyInjection;
using Security.Services;

namespace Security;

public static class ServiceProvider
{
    public static IServiceCollection AddSecurityLayer(this IServiceCollection collection,
        PasswordRules? passwordRules = null)
    {
        collection.AddSingleton<IJwtGenerator, JwtGenerator>();
        collection.AddSingleton<IPasswordManager, PasswordManager>();
        collection.AddSingleton(passwordRules ?? new PasswordRules());
        collection.AddSingleton<ITokenHasher, TokenHasher>();
        collection.AddScoped<IUserContext, UserContext>();

        return collection;
    }

    public static IServiceCollection AddSecurityLayer(this IServiceCollection collection, string secret, 
        JsonSerializerOptions? options = null, PasswordRules? passwordRules = null)
    {
        collection.AddSingleton<IJsonSigner, JsonSigner>(_ => new JsonSigner(secret, options));
        collection.AddSecurityLayer(passwordRules);
        return collection;
    }
}