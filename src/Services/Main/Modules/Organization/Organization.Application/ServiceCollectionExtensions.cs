using Application.Common.Host;
using Main.Entities.Organization;
using Main.Application.Configs;
using Main.Application.Services;
using Main.Modules.Organization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Organization.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrganizationApplication(this IServiceCollection services)
	{
		OrganizationSortByConfig.Configure();

		services.AddModuleApplication(OrganizationApplicationAssembly.Value);
		services.AddScoped<IOrganizationChecks, OrganizationChecks>();
		services.AddScoped<IOrganizationLocks, OrganizationLocks>();

		return services;
	}
}
