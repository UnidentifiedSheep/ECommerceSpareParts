using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Search.Application.Dtos.CatalogueCandidates;
using Search.Entities;

namespace Search.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class
	CatalogueCandidateDtoProjectionProvider : ProjectionProviderBase<CatalogueCandidate,
	CatalogueCandidateDto>
{
	public override Expression<Func<CatalogueCandidate, CatalogueCandidateDto>> Projection { get; } =
		candidate => new CatalogueCandidateDto
		{
			Id = candidate.Id,
			Sku = candidate.Sku,
			ProducerId = candidate.ProducerId,
			Names = candidate.Names
		};
}
