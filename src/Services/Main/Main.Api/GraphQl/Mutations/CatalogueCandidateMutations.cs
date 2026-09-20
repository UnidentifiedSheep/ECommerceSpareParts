using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.CatalogueCandidate;
using Main.Api.GraphQl.Types.Product;
using Main.Api.GraphQl.Types.ProductEnrichment;
using Main.Application.Handlers.ProductEnrichment;
using Main.Application.Handlers.ProductEnrichment.CreateCandidateCrosses;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class CatalogueCandidateMutations
{
	[RequireAllPermissions(PermissionCodes.CATALOGUE_CANDIDATES_REVIEW)]
	[GraphQLName("candidateToCatalogue")]
	public async Task<GqlProduct> AddCandidateToCatalogueAsync(
		ISender sender,
		GqlCandidateToCatalogueInput input,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new AddCandidateToCatalogueCommand(input.Id, input.SelectedName),
			cancellationToken);

		return new GqlProduct(result.CreatedIds[input.Id]);
	}

	[RequireAllPermissions(PermissionCodes.CATALOGUE_CANDIDATES_REVIEW)]
	[GraphQLName("mapCrosses")]
	public async Task<IReadOnlyList<GqlCatalogueCandidate>> MapCandidateCrossesAsync(
		ISender sender,
		GqlMaxCandidateCrossesInput input,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new CreateCandidateCrossesCommand(
				input.CandidateId,
				input.CrossCandidateIds,
				input.LinkageType),
			cancellationToken);

		var res = input.CrossCandidateIds
			.Select(x => new GqlCatalogueCandidate(x))
			.ToList();
		res.Add(new GqlCatalogueCandidate(input.CandidateId));

		return res;
	}
}
