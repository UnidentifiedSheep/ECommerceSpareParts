using Enums;
using Integrations.Supplier.Models;
using Pricing.Enums;

namespace Pricing.Application.Models;

public sealed record SupplierOfferExtractionResult
{
    public required Supplier Supplier { get; init; }
    public required IReadOnlyList<SupplierProduct> Offers { get; init; }
    public SupplierOfferExtractionStatus Status { get; init; }

    public static SupplierOfferExtractionResult SkippedByRefreshMarker(Supplier supplier)
        => EmptyCore(supplier, SupplierOfferExtractionStatus.SkippedByRefreshMarker);
    
    public static SupplierOfferExtractionResult AlreadyRefreshing(Supplier supplier)
        => EmptyCore(supplier, SupplierOfferExtractionStatus.AlreadyRefreshing);
    
    public static SupplierOfferExtractionResult NoSupplierReference(Supplier supplier)
        => EmptyCore(supplier, SupplierOfferExtractionStatus.NoSupplierReference);
    
    public static SupplierOfferExtractionResult SupplierRequestFailed(Supplier supplier)
        => EmptyCore(supplier, SupplierOfferExtractionStatus.SupplierRequestFailed);
    
    public static SupplierOfferExtractionResult Failed(Supplier supplier)
        => EmptyCore(supplier, SupplierOfferExtractionStatus.Failed);

    public static SupplierOfferExtractionResult Success(Supplier supplier, IEnumerable<SupplierProduct> offers)
        => new()
        {
            Supplier = supplier,
            Offers = offers.ToList(),
            Status = SupplierOfferExtractionStatus.Success,
        };
    
    private static SupplierOfferExtractionResult EmptyCore(Supplier supplier, SupplierOfferExtractionStatus status)
        => new()
        {
            Supplier = supplier,
            Status = status,
            Offers = [],
        };
}