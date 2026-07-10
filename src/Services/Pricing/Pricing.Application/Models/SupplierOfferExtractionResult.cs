using Enums;
using Integrations.Supplier.Models;
using Pricing.Enums;

namespace Pricing.Application.Models;

public sealed record SupplierOfferExtractionResult
{
    public required Supplier Supplier { get; init; }
    public required SupplierProduct? Offer { get; init; }
    public SupplierOfferExtractionStatus Status { get; init; }
    
    public bool IsSuccess => Status == SupplierOfferExtractionStatus.Success;

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
    
    public static SupplierOfferExtractionResult SupplierReturnedEmpty(Supplier supplier)
        => EmptyCore(supplier, SupplierOfferExtractionStatus.SupplierReturnedEmpty);

    public static SupplierOfferExtractionResult InvalidSupplierResponse(Supplier supplier)
        => EmptyCore(supplier, SupplierOfferExtractionStatus.InvalidSupplierResponse);
    
    public static SupplierOfferExtractionResult Success(Supplier supplier, SupplierProduct? offer)
        => new()
        {
            Supplier = supplier,
            Offer = offer,
            Status = SupplierOfferExtractionStatus.Success,
        };
    
    private static SupplierOfferExtractionResult EmptyCore(Supplier supplier, SupplierOfferExtractionStatus status)
        => new()
        {
            Supplier = supplier,
            Status = status,
            Offer = null,
        };
}