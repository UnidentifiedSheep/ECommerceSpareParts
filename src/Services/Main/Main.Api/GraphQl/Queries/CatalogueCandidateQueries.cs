using Abstractions.Models;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Application.Handlers.ProductEnrichment;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class CatalogueCandidateQueries
{
	[RequireAllPermissions(PermissionCodes.CATALOGUE_CANDIDATES_REVIEW)]
	[GraphQLName("byId")]
	[Lookup]
	public async Task<GqlCatalogueCandidate?> GetCandidateByIdAsync(
		ICatalogueCandidateByIdDataLoader loader,
		Guid id,
		CancellationToken ct)
	{
		var candidate = await loader.LoadAsync(id, ct);
		return candidate is null ? null : new GqlCatalogueCandidate(candidate);
	}

	[GraphQLName("byProductId")]
	[RequireAllPermissions(PermissionCodes.CATALOGUE_CANDIDATES_REVIEW)]
	public async Task<GqlCatalogueCandidate?> GetCandidateByProductIdAsync(
		ISender sender,
		[GraphQLName("productId")]
		int productId,
		CancellationToken cancellationToken)
	{
		var res = await sender.Send(
			new GetCatalogueCandidatesForReviewQuery(
				productId,
				null,
				new Pagination(0, 1)),
			cancellationToken);

		return res.Candidates.Count > 0 ? new GqlCatalogueCandidate(res.Candidates[0]) : null;
	}
}
