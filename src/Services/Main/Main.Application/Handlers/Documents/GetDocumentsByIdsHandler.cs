using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Exceptions;
using Main.Application.Dtos.Documents;
using Main.Entities;
using Main.Entities.Documents;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Documents;

public sealed record GetDocumentsByIdsQuery : IQuery<GetDocumentsByIdsResult>
{
	public IReadOnlyList<Guid> RequestIds { get; }
	public Guid? CallerId { get; }
	public bool CanAccessAll { get; }

	public GetDocumentsByIdsQuery(
		IEnumerable<Guid> requestIds,
		Guid? callerId,
		bool canAccessAll)
	{
		RequestIds = requestIds.Distinct().ToArray();
		CallerId = callerId;
		CanAccessAll = canAccessAll;
	}
}

public sealed record GetDocumentsByIdsResult(
	IReadOnlyDictionary<Guid, DocumentGenerationRequestDto> Documents);

public sealed class GetDocumentsByIdsHandler(
	IReadRepository<DocumentGenerationRequest, Guid> repository,
	IProjectionProvider<DocumentGenerationRequest, DocumentGenerationRequestDto> projection)
	: IQueryHandler<GetDocumentsByIdsQuery, GetDocumentsByIdsResult>
{
	public async Task<GetDocumentsByIdsResult> Handle(
		GetDocumentsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query;

		if (!request.CanAccessAll)
		{
			if (request.CallerId == null)
				throw new InvalidInputException(DocumentReadCallerIdRequiredMessage.Instance);

			query = query.Where(document => document.RequesterId == request.CallerId.Value);
		}

		if (request.RequestIds.Count == 0)
			return new GetDocumentsByIdsResult(new Dictionary<Guid, DocumentGenerationRequestDto>());

		var documents = await query
			.Where(document => request.RequestIds.Contains(document.RequestId))
			.Project(projection)
			.ToDictionaryAsync(document => document.RequestId, cancellationToken);

		return new GetDocumentsByIdsResult(documents);
	}
}
