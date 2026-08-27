using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product.Enrichment;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEnrichment;

public record GetCatalogueCandidatesByIdsQuery : IQuery<GetCatalogueCandidatesByIdsResult>
{
    public readonly IReadOnlyList<Guid> Ids;
    
    public GetCatalogueCandidatesByIdsQuery(IEnumerable<Guid> ids)
    {
        Ids = ids.Distinct().ToList();
    }
}

public record GetCatalogueCandidatesByIdsResult(
    IReadOnlyList<CatalogueCandidateReviewDto> Candidates);

public class GetCatalogueCandidatesByIdsHandler(
    IReadRepository<CatalogueCandidate, Guid> repository,
    IProjectionProvider<CatalogueCandidate, CatalogueCandidateReviewDto> projection)
    : IQueryHandler<GetCatalogueCandidatesByIdsQuery, GetCatalogueCandidatesByIdsResult>
{
    public async Task<GetCatalogueCandidatesByIdsResult> Handle(
        GetCatalogueCandidatesByIdsQuery request,
        CancellationToken cancellationToken)
        => new(await repository.Query
            .Where(x => request.Ids.Contains(x.Id))
            .Project(projection)
            .ToListAsync(cancellationToken: cancellationToken));
}