using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Document;
using Main.Api.GraphQl.Types.Inputs.Document;
using Main.Application.Dtos.Documents;
using Main.Application.Handlers.Documents;
using Main.Application.Handlers.Documents.SearchDocuments;
using MediatR;
using Security.Authorization;
using Security.Core.Interfaces;

namespace Main.Api.GraphQl.Queries;

public sealed class DocumentQueries
{
	private static readonly string AllDocumentsPermission =
		AuthorizationValueNormalizer.NormalizePermission(nameof(PermissionCodes.DOCUMENTS_ALL));

	[GraphQLName("available")]
	[RequireAnyPermission(PermissionCodes.DOCUMENTS_ME, PermissionCodes.DOCUMENTS_ALL)]
	public async Task<IReadOnlyList<GqlDocumentDefinition>> GetAvailableAsync(
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetDocumentDefinitionsQuery(), cancellationToken);
		return result.Definitions.Select(definition => new GqlDocumentDefinition(definition)).ToArray();
	}

	[GraphQLName("byId")]
	[Lookup]
	[RequireAnyPermission(PermissionCodes.DOCUMENTS_ME, PermissionCodes.DOCUMENTS_ALL)]
	public async Task<GqlDocumentGenerationRequest?> GetByIdAsync(
		Guid requestId,
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken)
	{
		var document = await loader.LoadAsync(requestId, cancellationToken);
		return document is null ? null : new GqlDocumentGenerationRequest(document);
	}

	[GraphQLName("byIds")]
	[RequireAnyPermission(PermissionCodes.DOCUMENTS_ME, PermissionCodes.DOCUMENTS_ALL)]
	public async Task<IReadOnlyList<GqlDocumentGenerationRequest>> GetByIdsAsync(
		IReadOnlyCollection<Guid> requestIds,
		IDocumentGenerationRequestsDataLoader loader,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(requestIds, cancellationToken))
			.OfType<DocumentGenerationRequestDto>()
			.Select(document => new GqlDocumentGenerationRequest(document))
			.ToArray();

	[GraphQLName("search")]
	[RequireAnyPermission(PermissionCodes.DOCUMENTS_ME, PermissionCodes.DOCUMENTS_ALL)]
	public async Task<IReadOnlyList<GqlDocumentGenerationRequest>> SearchAsync(
		GqlSearchDocumentsInput input,
		IUserContext userContext,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new SearchDocumentsQuery(
				input.RequesterId,
				userContext.UserId,
				input.DocumentSystemName,
				userContext.Permissions.Contains(AllDocumentsPermission),
				input.Pagination,
				input.SortBy?.Select(sort => sort.ToSortExpression()).ToArray()),
			cancellationToken);

		return result.Requests
			.Select(document => new GqlDocumentGenerationRequest(document))
			.ToArray();
	}
}
