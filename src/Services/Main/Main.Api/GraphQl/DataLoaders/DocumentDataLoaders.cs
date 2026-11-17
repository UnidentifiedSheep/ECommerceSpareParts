using Enums;
using GreenDonut;
using Main.Modules.Documents.Application.Dtos.Documents;
using Main.Modules.Documents.Application.Handlers.Documents;
using MediatR;
using Security.Authorization;
using Security.Core.Interfaces;

namespace Main.Api.GraphQl.DataLoaders;

public static class DocumentDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, DocumentGenerationRequestDto>>
		GetDocumentGenerationRequests(
			IReadOnlyList<Guid> keys,
			IUserContext userContext,
			ISender sender,
			CancellationToken cancellationToken)
		=> (await sender.Send(
				new GetDocumentsByIdsQuery(
					keys,
					userContext.UserId,
					userContext
						.Permissions
						.Contains(
							AuthorizationValueNormalizer
								.NormalizePermission(nameof(PermissionCodes.DOCUMENTS_ALL)))),
				cancellationToken))
			.Documents;
}
