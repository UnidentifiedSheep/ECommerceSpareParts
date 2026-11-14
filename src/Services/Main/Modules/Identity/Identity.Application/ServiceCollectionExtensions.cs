using Application.Common.Host;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Validators;
using Application.Common.NamedObject;
using Application.Common.Validators;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Application.Services;
using Main.Application.Services.PayloadProvider;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Identity.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
	{
		var assembly = IdentityApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly);

		services.AddSingleton<IJwtGenerator, JwtGenerator>();
		services.AddSingleton<IEmailValidator, EmailValidator>();
		services.AddScoped<IResetPayloadProvider, ResetPayloadProvider>();
		services.AddScoped<IVerificationPayloadProvider, VerificationPayloadProvider>();

		return services;
	}
}
