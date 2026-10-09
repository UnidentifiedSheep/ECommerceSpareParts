using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Documents;
using Main.Entities.Documents;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Documents.SearchDocuments;

public record SearchDocumentsQuery(
	Guid? RequesterId,
	Guid? CallerId,
	string? DocumentSystemName,
	bool CanAccessAll,
	Pagination Pagination,
	IReadOnlyCollection<string>? SortBy) : IQuery<SearchDocumentsResult>;
public record SearchDocumentsResult(IReadOnlyList<DocumentGenerationRequestDto> Requests);

public class SearchDocumentsHandler(
	IReadRepository<DocumentGenerationRequest, Guid> repository,
	IProjectionProvider<DocumentGenerationRequest, DocumentGenerationRequestDto> projection)
	: IQueryHandler<SearchDocumentsQuery, SearchDocumentsResult>
{
	public async Task<SearchDocumentsResult> Handle(
		SearchDocumentsQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query;

		if (!request.CanAccessAll)
		{
			if (request.CallerId is not { } callerId || callerId == Guid.Empty)
				throw new InvalidOperationException("Caller ID is required to search personal documents.");

			query = query.Where(x => x.RequesterId == callerId);
		}

		if (request.RequesterId is { } requesterId)
			query = query.Where(x => x.RequesterId == requesterId);

		if (!string.IsNullOrWhiteSpace(request.DocumentSystemName))
			query = query.Where(x => x.DocumentSystemName == request.DocumentSystemName);

		var result = await query
			.SortBy(request.SortBy)
			.ThenBy(x => x.RequestId)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new SearchDocumentsResult(result);
	}
}
