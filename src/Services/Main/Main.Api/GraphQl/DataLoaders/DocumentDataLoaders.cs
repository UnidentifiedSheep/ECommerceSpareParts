using Enums;
using GreenDonut;
using Main.Application.Dtos.Documents;
using Main.Application.Handlers.Documents;
using MediatR;
using Security.Authorization;
using Security.Core.Interfaces;

namespace Main.Api.GraphQl.DataLoaders;

public static class DocumentDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, DocumentGenerationRequestDto>> GetDocumentGenerationRequests(
		IReadOnlyList<Guid> keys,
		IUserContext userContext,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(
				new GetDocumentsByIdsQuery(
					requestIds: keys,
					callerId: userContext.UserId,
					canAccessAll: userContext
						.Permissions
						.Contains(AuthorizationValueNormalizer
							.NormalizePermission(nameof(PermissionCodes.DOCUMENTS_ALL)))),
				cancellationToken))
			.Documents;
}
