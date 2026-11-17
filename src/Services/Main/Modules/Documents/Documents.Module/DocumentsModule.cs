using System.Reflection;
using Application.Common.Host;
using Main.Modules.Documents.Application;
using Main.Modules.Documents.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Documents;

public sealed class DocumentsModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies => [DocumentsApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddDocumentsApplication();
		services.AddDocumentLinks(configuration);
		services.AddDocumentsPersistence();
	}
}
