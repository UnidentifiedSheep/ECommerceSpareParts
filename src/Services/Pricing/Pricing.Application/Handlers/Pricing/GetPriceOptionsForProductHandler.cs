using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Entities;
using Pricing.Entities.Offers;
using Pricing.Entities.Settings;
using Pricing.Enums;

namespace Pricing.Application.Handlers.Pricing;

public record GetPriceOptionsForProductQuery(
    int ProductId,
    int CurrencyId,
    string StorageName,
    IEnumerable<PriceOfferSource> Sources,
    Pagination Pagination,
    string? SortBy) : IQuery<GetPriceOptionsForProductResult>;

public record GetPriceOptionsForProductResult(IReadOnlyCollection<PriceOptionDto> PriceOptions);

public class GetPriceOptionsForProductHandler(
    ISender sender,
    IReadRepository<ProductPriceOption, Guid> repository,
    ICurrencyConverter currencyConverter,
    IMarkupContainer markupContainer,
    IPriceApplierService priceApplierService,
    ISettingsService settingsService
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
            await CalculateCandidates(request, cancellationToken);
        
        var options = await GetOptionsAsync(request, cancellationToken);
        var appliersVersion = await priceApplierService
            .GetCurrentConfigurationVersionAsync(cancellationToken);
        var pricingSettingsVersion = (await settingsService
            .GetOrDefault<PricingSetting>(cancellationToken)).Data.Version;

        if (options.Count == 0 || options.Any(x =>
                x.MarkupVersion != markupContainer.CurrentVersion
                || x.AppliersVersion != appliersVersion
                || x.PricingSettingsVersion != pricingSettingsVersion))
        {
            await CalculateCandidates(request, cancellationToken);
            options = await GetOptionsAsync(request, cancellationToken);
        }
        
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
                    DeliveryProbability = option.PriceOffer.DeliveryProbability,
                    ExpiresAt = option.PriceOffer.ExpiresAt,
                    DeliveryDate = option.PriceOffer.DeliveryDate,
                    GuaranteedDeliveryDate = option.PriceOffer.GuaranteedDeliveryDate,
                    OrderTill = option.PriceOffer.OrderTill,
                    Id = option.PriceOffer.Id,
                    MinimumPurchaseQuantity = option.PriceOffer.MinimumPurchaseQuantity,
                    OfferForStorage = option.PriceOffer.OfferForStorage,
                    ProductId = option.PriceOffer.ProductId,
                    PurchasePrice = option.PriceOffer.PurchasePrice,
                    Source = option.PriceOffer.Source,
                    QuantityCoefficient = option.PriceOffer.QuantityCoefficient,
                }
            });
        }
        
        return new GetPriceOptionsForProductResult(result);
    }

    private async Task CalculateCandidates(
        GetPriceOptionsForProductQuery request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new CalculateCandidatesCommand(request.ProductId, request.StorageName), 
            cancellationToken);
    }

    private async Task<List<ProductPriceOption>> GetOptionsAsync(
        GetPriceOptionsForProductQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query
            .Include(x => x.PriceOffer)
            .Where(x => x.PriceOffer.ProductId == request.ProductId)
            .Where(x => x.PriceOffer.OfferForStorage == request.StorageName)
            .Where(x => x.PriceOffer.ExpiresAt > DateTime.UtcNow);

        if (request.Sources.Any())
            query = query.Where(x => request.Sources.Contains(x.PriceOffer.Source));
            
        return await query
            .SortBy(request.SortBy)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
    }
}
