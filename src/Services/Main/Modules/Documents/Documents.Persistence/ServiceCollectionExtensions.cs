using Main.Entities.Documents.Documents;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Extensions;

namespace Main.Modules.Documents.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDocumentsPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<DocumentsDbContextModule>()
			.AddEntities(typeof(DocumentGenerationRequest).Assembly);
}
