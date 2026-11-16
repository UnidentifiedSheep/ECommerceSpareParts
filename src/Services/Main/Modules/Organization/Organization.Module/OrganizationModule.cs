using System.Reflection;
using Application.Common.Host;
using Main.Modules.Organization.Application;
using Main.Modules.Organization.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Organization;

public sealed class OrganizationModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies
		=> [OrganizationApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddOrganizationApplication();
		services.AddOrganizationPersistence();
	}
}
