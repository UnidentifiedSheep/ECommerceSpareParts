using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.CatalogueCandidate;
using Main.Application.Handlers.ProductEnrichment;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class CatalogueCandidateMutations
{
	[RequireAllPermissions(PermissionCodes.CATALOGUE_CANDIDATES_REVIEW)]
	[GraphQLName("candidateToCatalogue")]
	public async Task<bool> AddCandidateToCatalogueAsync(
		ISender sender,
		GqlCandidateToCatalogueInput input,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new AddCandidateToCatalogueCommand(
				input.Id,
				input.SelectedName),
			cancellationToken);

		return true;
	}
}
