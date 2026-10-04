using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Document;
using Main.Application.Handlers.Documents.GetDocumentDefinitions;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class DocumentQueries
{
	[GraphQLName("available")]
	[RequireAnyPermission(PermissionCodes.DOCUMENTS_ME, PermissionCodes.DOCUMENTS_ALL)]
	public async Task<IReadOnlyList<GqlDocumentDefinition>> GetAvailableAsync(
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetDocumentDefinitionsQuery(), cancellationToken);
		return result.Definitions.Select(definition => new GqlDocumentDefinition(definition)).ToArray();
	}
}
