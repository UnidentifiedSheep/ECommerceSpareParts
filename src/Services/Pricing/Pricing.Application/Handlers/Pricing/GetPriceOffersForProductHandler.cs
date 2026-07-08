using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing;

public record GetPriceOffersForProductQuery(
    int ProductId,
    int CurrencyId,
    string StorageName,
    Pagination Pagination) : IQuery<GetPriceOffersForProductResult>;

public record GetPriceOffersForProductResult(IReadOnlyCollection<CalculatedScoredPriceCandidate> Candidates);

public class GetPriceOffersForProductHandler(
    ISender sender,
    IReadRepository<PriceOffer, Guid> repository,
    IProductPriceCalculator calculator,
    IPriceCandidateBuilder builder
    ) : IQueryHandler<GetPriceOffersForProductQuery, GetPriceOffersForProductResult>
{
    public async Task<GetPriceOffersForProductResult> Handle(
        GetPriceOffersForProductQuery request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new RefreshOffersCommand(request.ProductId, request.StorageName),
            cancellationToken);

        var result = await repository.Query
            .AsExpandable()
            .Where(x => x.ProductId == request.ProductId)
            .Where(x => x.OfferForStorage == request.StorageName)
            .ToListAsync(cancellationToken);

        var candidates = await builder.Build(
            result,
            request.StorageName,
            cancellationToken);
        
        var calculated = await calculator
            .CalculateAsync(candidates, cancellationToken);

        return new GetPriceOffersForProductResult(calculated);
    }
}
