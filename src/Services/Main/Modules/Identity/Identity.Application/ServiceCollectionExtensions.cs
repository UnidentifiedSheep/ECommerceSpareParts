using Application.Common.Host;
using Application.Common.Interfaces;
using Main.Modules.Identity.Abstractions.Interfaces;
using Main.Modules.Identity.Abstractions.Models;
using Main.Modules.Identity.Application.Configs;
using Main.Modules.Identity.Application.Interfaces.Services;
using Main.Modules.Identity.Application.Interfaces.Services.PayloadProvider;
using Main.Modules.Identity.Application.Models.Options;
using Main.Modules.Identity.Application.Notifications;
using Main.Modules.Identity.Application.Services;
using Main.Modules.Identity.Application.Services.PayloadProvider;
using Main.Modules.Identity.Application.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;
using Notification.Core;
using EmailValidator = Main.Modules.Identity.Application.Validators.EmailValidator;

namespace Main.Modules.Identity.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityLinks(
		this IServiceCollection services,
		IConfiguration configuration)
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

		services.AddNotification<PasswordResetNotification, PasswordResetNotificationData>(
			"PasswordReset",
			model => new PasswordResetNotification(model));
		services.AddNotification<EmailVerificationNotification, EmailVerificationNotificationData>(
			"EmailVerification",
			model => new EmailVerificationNotification(model));
		services.AddNotification<UserLoggedInNotification, UserLoggedInNotificationData>(
			"UserLoggedIn",
			model => new UserLoggedInNotification(model));

		var assembly = IdentityApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly);

		services.AddScoped<IUserChecks, UserChecks>();
		services.AddScoped<IUserEmailLookup, UserEmailLookup>();
		services.AddScoped<IUserSearch, UserSearch>();
		services.AddScoped<IDisplay<Guid, UserDisplayInfo>, UserDisplay>();
		services.AddSingleton<IJwtGenerator, JwtGenerator>();
		services.AddSingleton<IEmailValidator, EmailValidator>();
		services.AddScoped<IResetPayloadProvider, ResetPayloadProvider>();
		services.AddScoped<IVerificationPayloadProvider, VerificationPayloadProvider>();

		return services;
	}
}
