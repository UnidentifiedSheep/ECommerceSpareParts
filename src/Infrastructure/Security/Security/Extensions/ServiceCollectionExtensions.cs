using System.Text;
using Abstractions.Models.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Security.Authorization;
using Security.Core.Interfaces;
using Security.Models;
using Security.Services;

namespace Security.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddFullSecurityLayer(
		this IServiceCollection collection,
		PasswordRules? passwordRules = null)
	{
		collection.AddPasswordServices(passwordRules);
		collection.AddMinimalSecurityLayer();
		return collection;
	}

	public static IServiceCollection AddPasswordServices(
		this IServiceCollection collection,
		PasswordRules? passwordRules = null)
	{
		collection.TryAddSingleton(passwordRules ?? new PasswordRules());
		collection.TryAddSingleton<Hasher>();
		collection.TryAddSingleton<IValueHasher>(provider => provider.GetRequiredService<Hasher>());
		collection.TryAddSingleton<IPasswordHasher>(provider => provider.GetRequiredService<Hasher>());
		collection.TryAddSingleton<IPasswordManager, PasswordManager>();
		return collection;
	}

	public static IServiceCollection AddJsonSigner(this IServiceCollection collection)
	{
		collection.TryAddSingleton<ProjectJsonOptions>();
		collection.TryAddSingleton<IJsonSigner, JsonSigner>();
		return collection;
	}

	public static IServiceCollection AddSecretEncryptor(this IServiceCollection collection)
	{
		collection.TryAddSingleton<ISecretEncryptor, SecretEncryptor>();
		return collection;
	}

	public static IServiceCollection AddMinimalSecurityLayer(this IServiceCollection collection)
	{
		collection.AddHttpContextAccessor();
		collection.TryAddScoped<IUserContext, UserContext>();
		collection.TryAddEnumerable(
			ServiceDescriptor.Scoped<IAuthorizationHandler, PermissionAuthorizationHandler>());
		collection.TryAddEnumerable(
			ServiceDescriptor.Scoped<IAuthorizationHandler, RoleAuthorizationHandler>());
		return collection;
	}

	public static IServiceCollection AddWorkerSecurityLayer(this IServiceCollection collection)
	{
		collection.TryAddSingleton<IUserContext, WorkerUserContext>();
		return collection;
	}

	public static IServiceCollection AddEComAuth(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var issuer = configuration["JwtBearer:ValidIssuer"];

		var tokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = false,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = issuer,
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(
					configuration["JwtBearer:IssuerSigningKey"]!))
		};

		services
			.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme =
					JwtBearerDefaults.AuthenticationScheme;

				options.DefaultChallengeScheme =
					JwtBearerDefaults.AuthenticationScheme;

				options.DefaultScheme =
					JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(
				JwtBearerDefaults.AuthenticationScheme,
				options =>
				{
					options.TokenValidationParameters = tokenValidationParameters;
				})
			.AddJwtBearer(
				AuthenticationSchemes.WebSocketBearer,
				options =>
				{
					options.TokenValidationParameters = tokenValidationParameters;
				});

		services
			.AddAuthorizationBuilder()
			.SetDefaultPolicy(
				new AuthorizationPolicyBuilder(
						JwtBearerDefaults.AuthenticationScheme)
					.RequireAuthenticatedUser()
					.Build());

		return services;
	}
}
