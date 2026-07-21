using Exceptions.Base.Localized;

namespace Pricing.Entities.Exceptions;

public class PriceApplierNotFoundException(string systemName)
    : LocalizedNotFoundException(
        "price.applier.not.found",
        new { SystemName = systemName });

public class LocalPriceApplierCannotBeDeletedException(string systemName)
    : LocalizedBadRequestException(
        "price.applier.local.cannot.be.deleted",
        new { SystemName = systemName });
