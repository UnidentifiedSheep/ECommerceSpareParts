using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Price;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing;

public record GetPriceOptionsForProductQuery(
    int ProductId,
    int CurrencyId,
    string StorageName,
    Pagination Pagination,
    string? SortBy) : IQuery<GetPriceOptionsForProductResult>;

public record GetPriceOptionsForProductResult(IReadOnlyCollection<PriceOptionDto> PriceOptions);

public class GetPriceOptionsForProductHandler(
    ISender sender,
    IReadRepository<ProductPriceOption, Guid> repository,
    ICurrencyConverter currencyConverter
    ) : IQueryHandler<GetPriceOptionsForProductQuery, GetPriceOptionsForProductResult>
{
    public async Task<GetPriceOptionsForProductResult> Handle(
        GetPriceOptionsForProductQuery request,
        CancellationToken cancellationToken)
    {
        var refreshed = await sender.Send(
            new RefreshOffersCommand(request.ProductId, request.StorageName),
            cancellationToken);

        if (refreshed.CreatedOffers.Count != 0)
            await sender.Send(
                new CalculateCandidatesCommand(request.ProductId, request.StorageName), 
                cancellationToken);
        
        var options = await repository.Query
            .Include(x => x.PriceOffer)
            .Where(x => x.PriceOffer.ProductId == request.ProductId)
            .Where(x => x.PriceOffer.OfferForStorage == request.StorageName)
            .Where(x => x.PriceOffer.ExpiresAt > DateTime.UtcNow)
            .SortBy(request.SortBy)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        var result = new List<PriceOptionDto>();

        foreach (var option in options)
        {
            result.Add(new PriceOptionDto
            {
                CurrencyId = request.CurrencyId,
                DeliveryProbability = option.DeliveryProbability,
                DeliveryTime = option.DeliveryTime,
                GuaranteedDeliveryTime = option.GuaranteedDeliveryTime,
                ForStorageName = option.ForStorageName,
                Markup = option.Markup,
                Price = await currencyConverter.ConvertAsync(option.Price, option.CurrencyId, request.CurrencyId, cancellationToken),
                Score = option.Score,
                PriceOfferId = option.PriceOfferId,
                PriceOffer = new PriceOfferDto
                {
                    AvailableQuantity = option.PriceOffer.AvailableQuantity,
                    CurrencyId = option.PriceOffer.CurrencyId,
                    DaysToRefund = option.PriceOffer.DaysToRefund,
                    DeliveryDate = option.PriceOffer.DeliveryDate,
                    DeliveryProbability = option.PriceOffer.DeliveryProbability,
                    ExpiresAt = option.PriceOffer.ExpiresAt,
                    GuaranteedDeliveryDate = option.PriceOffer.GuaranteedDeliveryDate,
                    Id = option.PriceOffer.Id,
                    MinimumPurchaseQuantity = option.PriceOffer.MinimumPurchaseQuantity,
                    OfferForStorage = option.PriceOffer.OfferForStorage,
                    ProductId = option.PriceOffer.ProductId,
                    PurchasePrice = option.PriceOffer.PurchasePrice,
                    Source = option.PriceOffer.Source,
                    QuantityCoefficient = option.PriceOffer.QuantityCoefficient,
                    OrderTill = option.PriceOffer.OrderTill
                }
            });
        }
        
        return new GetPriceOptionsForProductResult(result);
    }
}
