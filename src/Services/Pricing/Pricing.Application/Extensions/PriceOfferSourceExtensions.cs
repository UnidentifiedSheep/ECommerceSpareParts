using Enums;
using Pricing.Enums;

namespace Pricing.Application.Extensions;

public static class PriceOfferSourceExtensions
{
    public static PriceOfferSource ToSource(this Supplier supplier)
    {
        return supplier switch
        {
            Supplier.Armtek => PriceOfferSource.Armtek,
            Supplier.FavoritParts => PriceOfferSource.FavoriteParts,
            Supplier.Tmtr => PriceOfferSource.Tmtr,
            _ => throw new ArgumentOutOfRangeException(nameof(supplier), supplier, null)
        };
    }

    public static PriceOfferSourceType GetSourceType(this PriceOfferSource source)
        => source == PriceOfferSource.OurWarehouse ? PriceOfferSourceType.OurWarehouse : PriceOfferSourceType.Supplier;
}