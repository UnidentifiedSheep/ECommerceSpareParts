using Enums;
using Pricing.Enums;

namespace Pricing.Application.Extensions;

public static class PriceOfferSourceExtensions
{
    public static PriceOfferSource GetFromSupplier(this Supplier supplier)
    {
        return supplier == Supplier.Armtek
            ? PriceOfferSource.Armtek
            : PriceOfferSource.FavoriteParts;
    }
}