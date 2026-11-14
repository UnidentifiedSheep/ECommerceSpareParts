using System.Reflection;
using Application.Common.Host;
using Main.Modules.Identity.Application;
using Main.Modules.Identity.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Identity;

public sealed class IdentityModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [IdentityApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddIdentityApplication();
		services.AddIdentityPersistence();
	}
}
