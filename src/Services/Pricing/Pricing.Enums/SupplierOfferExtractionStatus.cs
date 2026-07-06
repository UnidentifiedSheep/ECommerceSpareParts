namespace Pricing.Enums;

public enum SupplierOfferExtractionStatus
{
    Success,

    SkippedByRefreshMarker,
    AlreadyRefreshing,

    NoSupplierReference,

    SupplierRequestFailed,

    Failed
}