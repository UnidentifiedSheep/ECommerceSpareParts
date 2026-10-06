using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Application.Dtos.Documents;
using Main.Entities.Documents;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class DocumentGenerationRequestDtoProjectionProvider
	: ProjectionProviderBase<DocumentGenerationRequest, DocumentGenerationRequestDto>
{
	public override Expression<Func<DocumentGenerationRequest, DocumentGenerationRequestDto>> Projection { get; } =
		request => new DocumentGenerationRequestDto
		{
			RequestId = request.RequestId,
			JobId = request.JobId,
			Status = request.Job.Status,
			DocumentSystemName = request.DocumentSystemName,
			RequesterId = request.RequesterId,
			CreatedAt = request.CreatedAt,
			GeneratedAt = request.GeneratedAt,
			ExpiresAt = request.ExpiresAt,
			StorageKey = request.StorageKey,
			BucketName = request.BucketName
		};
}
