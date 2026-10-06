using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Document;
using Main.Api.GraphQl.Types.Inputs.Document;
using Main.Application.Handlers.Documents.CreateGenerationRequest;
using MediatR;
using Security.Core.Interfaces;

namespace Main.Api.GraphQl.Mutations;

public sealed class DocumentMutations
{
	[GraphQLName("createGenerationRequest")]
	[RequireAnyPermission(PermissionCodes.DOCUMENTS_ME, PermissionCodes.DOCUMENTS_ALL)]
	public async Task<GqlDocumentGenerationRequest> CreateGenerationRequestAsync(
		IUserContext userContext,
		ISender sender,
		GqlCreateGenerationRequestInput input,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateGenerationRequestCommand(
				input.DocumentSystemName,
				input.DocumentRequest,
				userContext.UserId),
			cancellationToken);

		return new GqlDocumentGenerationRequest(result.RequestId);
	}
}
