using Enums;
using Pricing.Entities.Enums;

namespace Pricing.Application.Extensions;

public static class PriceOfferSourceExtensions
{
	public static PriceOfferSource ToSource(this Supplier supplier)
		=> supplier switch
		{
			Supplier.Armtek => PriceOfferSource.Armtek,
			Supplier.FavoritParts => PriceOfferSource.FavoriteParts,
			Supplier.Tmtr => PriceOfferSource.Tmtr,
			_ => throw new ArgumentOutOfRangeException(
				nameof(supplier),
				supplier,
				null)
		};

	public static Supplier? ToSupplier(this PriceOfferSource source)
		=> source switch
		{
			PriceOfferSource.Armtek => Supplier.Armtek,
			PriceOfferSource.FavoriteParts => Supplier.FavoritParts,
			PriceOfferSource.Tmtr => Supplier.Tmtr,
			_ => null
		};

	public static PriceOfferSourceType GetSourceType(this PriceOfferSource source)
		=> source == PriceOfferSource.OurWarehouse
			? PriceOfferSourceType.OurWarehouse
			: PriceOfferSourceType.Supplier;
}
