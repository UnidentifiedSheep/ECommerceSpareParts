using System.Reflection;
using Application.Common.Host;
using Main.Modules.Uploads.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Uploads;

public sealed class UploadsModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [UploadsApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		=> services.AddUploadsApplication();
}
