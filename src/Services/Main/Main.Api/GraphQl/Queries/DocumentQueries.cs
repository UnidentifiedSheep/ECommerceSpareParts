using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Document;
using Main.Application.Handlers.Documents.GetDocumentDefinitions;
using Main.Application.Handlers.Documents.GetDocumentLink;
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

	[GraphQLName("link")]
	[RequireAnyPermission(PermissionCodes.DOCUMENTS_ME, PermissionCodes.DOCUMENTS_ALL)]
	public async Task<GqlDocumentLink> GetLinkAsync(
		Guid requestId,
		IUserContext userContext,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetDocumentLinkQuery(
				requestId,
				userContext.UserId,
				userContext.Permissions.Contains(AllDocumentsPermission)),
			cancellationToken);

		return new GqlDocumentLink(result.Url, result.UrlExpiresAtUtc);
	}
}
