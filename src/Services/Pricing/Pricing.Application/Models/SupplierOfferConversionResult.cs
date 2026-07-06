using Enums;
using Pricing.Entities;

namespace Pricing.Application.Models;

public record SupplierOfferConversionResult
{
    public required Supplier Supplier { get; init; }
    public required IReadOnlyList<PriceOffer> Offers { get; init; }
    public required IReadOnlyList<string> NotFoundCurrencies { get; init; }
}