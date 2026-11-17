namespace Pricing.Entities.Enums;

public enum SupplierOfferExtractionStatus
{
	Success,

	SkippedByRefreshMarker,

	AlreadyRefreshing,

	NoSupplierReference,

	SupplierRequestFailed,

	SupplierReturnedEmpty,

	InvalidSupplierResponse,

	Failed
}
