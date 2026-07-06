using Enums;
using Integrations.Supplier.Models;
using Pricing.Enums;

namespace Pricing.Application.Models;

public sealed record SupplierOfferExtractionResult
{
    public required Supplier Supplier { get; init; }
    public required IReadOnlyList<SupplierProduct> Offers { get; init; }
    public SupplierOfferExtractionStatus Status { get; init; }
}