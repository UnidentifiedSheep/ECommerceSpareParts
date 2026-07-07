using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Price;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing;

public record GetPriceOffersForProductQuery(
    int ProductId,
    int CurrencyId,
    string StorageName,
    Pagination Pagination) : IQuery<GetPriceOffersForProductResult>;

public record GetPriceOffersForProductResult(IReadOnlyList<PriceOfferDto> PriceOffers);

public class GetPriceOffersForProductHandler(
    ISender sender,
    IReadRepository<PriceOffer, Guid> repository
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
            .Select(x => new PriceOfferDto
            {
                Id = x.Id,
                AvailableQuantity = x.AvailableQuantity,
                DaysToRefund = x.DaysToRefund,
                DeliveryDate = x.DeliveryDate,
                DeliveryProbability = x.DeliveryProbability,
                ExpiresAt = x.ExpiresAt,
                GuaranteedDeliveryDate = x.GuaranteedDeliveryDate,
                MinimumPurchaseQuantity = x.MinimumPurchaseQuantity,
                OfferForStorage = x.OfferForStorage,
                OfferCurrencyId = x.CurrencyId,
                ProductId = x.ProductId,
                OfferPrice = x.Price,
                QuantityCoefficient = x.QuantityCoefficient,
                Source = x.Source,
                SourceKey = x.SourceKey,
                UpdatedAt = x.UpdatedAt,
                OrderTill = x.OrderTill
            })
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetPriceOffersForProductResult(result);
    }
}
