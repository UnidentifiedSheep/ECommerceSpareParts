using Main.Modules.Identity.Application.Configs;
using Application.Common.Host;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Validators;
using Application.Common.Validators;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Application.Services;
using Main.Application.Models.Options;
using Main.Modules.Identity.Abstractions;
using Microsoft.Extensions.Configuration;
using Main.Application.Services.PayloadProvider;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Identity.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityLinks(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<IdentityLinksOptions>()
			.Bind(configuration.GetSection(IdentityLinksOptions.SectionName))
			.Validate(
				options => options.IsValid(),
				"Auth link templates must be relative and contain their required placeholders.")
			.ValidateOnStart();
		services.AddScoped<IAuthLinkProvider, AuthLinkProvider>();

		return services;
	}

	public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
	{
		services.AddValidationErrors(IdentityValidationConfiguration.Configure);

		var assembly = IdentityApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly);

		services.AddScoped<IUserChecks, UserChecks>();
		services.AddScoped<IUserEmailLookup, UserEmailLookup>();
		services.AddScoped<IUserSearch, UserSearch>();
		services.AddScoped<IUserNames, UserNames>();
		services.AddSingleton<IJwtGenerator, JwtGenerator>();
		services.AddSingleton<IEmailValidator, EmailValidator>();
		services.AddScoped<IResetPayloadProvider, ResetPayloadProvider>();
		services.AddScoped<IVerificationPayloadProvider, VerificationPayloadProvider>();

		return services;
	}
}
