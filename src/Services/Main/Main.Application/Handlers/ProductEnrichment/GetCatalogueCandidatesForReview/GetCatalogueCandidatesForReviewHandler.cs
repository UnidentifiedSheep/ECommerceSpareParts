using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product.Enrichment;
using Main.Entities.Product.Enrichment;
using ProductSku = Main.Entities.Product.ValueObjects.Sku;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEnrichment.GetCatalogueCandidatesForReview;

public record GetCatalogueCandidatesForReviewQuery(
    int? ProductId,
    string? Sku,
    Pagination Pagination
) : IQuery<GetCatalogueCandidatesForReviewResult>;

public record GetCatalogueCandidatesForReviewResult(
    IReadOnlyList<CatalogueCandidateReviewDto> Candidates);

public sealed class GetCatalogueCandidatesForReviewHandler(
    IReadRepository<CatalogueCandidate, int> repository,
    IProjectionProvider<CatalogueCandidate, CatalogueCandidateReviewDto> projection)
    : IQueryHandler<GetCatalogueCandidatesForReviewQuery, GetCatalogueCandidatesForReviewResult>
{
    public async Task<GetCatalogueCandidatesForReviewResult> Handle(
        GetCatalogueCandidatesForReviewQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.ProductId.HasValue)
            query = query.Where(x => x.ProductId == request.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var normalizedSku = ProductSku.ToNormalized(request.Sku);
            query = query.Where(x => x.Sku.NormalizedValue == normalizedSku);
        }

        var candidates = await query
            .OrderBy(x => x.Id)
            .Project(projection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetCatalogueCandidatesForReviewResult(candidates);
    }
}
