using Application.Common.Querying;
using Main.Entities.Documents;
using Main.Entities.Documents.Documents;

namespace Main.Modules.Documents.Application.Configs;

public static class DocumentsSortByConfig
{
	public static void Configure()
	{
		QueryableSortBy
			.Value
			.MapDefault<DocumentGenerationRequest, DateTime>(x => x.CreatedAt, true)
			.Map<DocumentGenerationRequest, Guid>("requestId", x => x.RequestId)
			.Map<DocumentGenerationRequest, Guid>("jobId", x => x.JobId)
			.Map<DocumentGenerationRequest, Guid?>("requesterId", x => x.RequesterId)
			.Map<DocumentGenerationRequest, string>("documentSystemName", x => x.DocumentSystemName)
			.Map<DocumentGenerationRequest, DateTime>("createdAt", x => x.CreatedAt)
			.Map<DocumentGenerationRequest, DateTime?>("generatedAt", x => x.GeneratedAt)
			.Map<DocumentGenerationRequest, DateTime?>("expiresAt", x => x.ExpiresAt);
	}
}
