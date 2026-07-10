using Enums;
using Pricing.Enums;

namespace Pricing.Application.Extensions;

public static class PriceOfferSourceExtensions
{
    public static PriceOfferSource ToSource(this Supplier supplier)
    {
        return supplier == Supplier.Armtek
            ? PriceOfferSource.Armtek
            : PriceOfferSource.FavoriteParts;
    }

    public static PriceOfferSourceType GetSourceType(this PriceOfferSource source)
        => source == PriceOfferSource.OurWarehouse ? PriceOfferSourceType.OurWarehouse : PriceOfferSourceType.Supplier;
}