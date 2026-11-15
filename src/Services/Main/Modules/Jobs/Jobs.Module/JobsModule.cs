using System.Reflection;
using Application.Common.Host;
using Main.Modules.Jobs.Application;
using Main.Modules.Jobs.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Jobs;

public sealed class JobsModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [JobsApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddJobsApplication();
		services.AddJobsPersistence();
	}
}
