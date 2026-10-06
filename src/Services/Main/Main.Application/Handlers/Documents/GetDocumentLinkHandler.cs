using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEnums;
using Main.Application.Dtos.Documents;
using Main.Application.Interfaces.Cache;
using Main.Entities.Documents;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Documents;

public sealed record GetDocumentLinkQuery(
	Guid RequestId,
	Guid CallerId,
	bool CanAccessAll) : IQuery<GetDocumentLinkResult>;

public sealed class GetDocumentLinkHandler(
	IReadRepository<DocumentGenerationRequest, Guid> repository,
	IDocumentLinkProvider linkProvider)
	: IQueryHandler<GetDocumentLinkQuery, GetDocumentLinkResult>
{
	public async Task<GetDocumentLinkResult> Handle(
		GetDocumentLinkQuery request,
		CancellationToken cancellationToken)
	{
		var document = await repository.Query
			.Where(document => document.RequestId == request.RequestId &&
			                   (request.CanAccessAll || document.RequesterId == request.CallerId))
			.Select(document => new
			{
				document.BucketName,
				document.StorageKey,
				document.ExpiresAtUtc,
				JobStatus = document.Job.Status
			})
			.FirstOrDefaultAsync(cancellationToken);

		if (document is null)
			throw new DocumentGenerationRequestNotFoundException(request.RequestId);

		var now = DateTime.UtcNow;
		if (document.ExpiresAtUtc is { } expiresAtUtc && expiresAtUtc <= now)
			throw new DocumentGenerationRequestNotFoundException(request.RequestId);

		if (document.BucketName is null || document.StorageKey is null)
			throw document.JobStatus switch
			{
				JobStatus.Failed => new DocumentGenerationFailedException(request.RequestId),
				JobStatus.Cancelled => new DocumentGenerationCancelledException(request.RequestId),
				_ => new DocumentNotReadyException(request.RequestId)
			};

		if (document.ExpiresAtUtc is null)
			throw new InvalidOperationException("Generated document expiration time is missing.");

		return await linkProvider.GetOrCreateAsync(
			request.RequestId,
			document.BucketName,
			document.StorageKey,
			document.ExpiresAtUtc.Value);
	}
}
