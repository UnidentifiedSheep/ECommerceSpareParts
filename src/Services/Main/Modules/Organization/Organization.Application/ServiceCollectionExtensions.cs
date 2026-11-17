using Application.Common.Host;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Organization.Application.Configs;
using Main.Modules.Organization.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Organization.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrganizationApplication(this IServiceCollection services)
	{
		services.AddValidationErrors(OrganizationValidationConfiguration.Configure);

		OrganizationSortByConfig.Configure();

		services.AddModuleApplication(OrganizationApplicationAssembly.Value);
		services.AddScoped<IOrganizationChecks, OrganizationChecks>();
		services.AddScoped<IOrganizationLocks, OrganizationLocks>();
		services.AddScoped<IOrganizationSearch, OrganizationSearch>();
		services.AddScoped<IOrganizationNames, OrganizationNames>();

		return services;
	}
}
