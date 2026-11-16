using Persistence.Common;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Extensions;

namespace Main.Modules.Documents.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDocumentsPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<DocumentsDbContextModule>().AddEntities(typeof(Main.Entities.Documents.DocumentGenerationRequest).Assembly);
}
