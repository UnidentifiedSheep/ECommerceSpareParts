using System.Reflection;
using Application.Common.Host;
using Main.Modules.Storage.Application;
using Main.Modules.Storage.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Storage;

public sealed class StorageModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [StorageApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddStorageApplication();
		services.AddStoragePersistence();
	}
}
