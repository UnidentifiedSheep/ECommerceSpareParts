using Core.Models;
using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.StorageRoutes;

public class PatchStorageRouteDto
{
    public PatchField<int> DistanceM { get; set; } = PatchField<int>.NotSet();

    public PatchField<RouteType> RouteType { get; set; } = PatchField<RouteType>.NotSet();

    public PatchField<LogisticPricingType> PricingModel { get; set; } = PatchField<LogisticPricingType>.NotSet();

    public PatchField<int> DeliveryTimeMinutes { get; set; } = PatchField<int>.NotSet();

    public PatchField<decimal> PriceKg { get; set; } = PatchField<decimal>.NotSet();

    public PatchField<decimal> PricePerM3 { get; set; } = PatchField<decimal>.NotSet();

    public PatchField<decimal> PricePerOrder { get; set; } = PatchField<decimal>.NotSet();

    public PatchField<bool> IsActive { get; set; } = PatchField<bool>.NotSet();

    public PatchField<int> CurrencyId { get; set; } = PatchField<int>.NotSet();

    public PatchField<decimal?> MinimumPrice { get; set; } = PatchField<decimal?>.NotSet();
    public PatchField<Guid?> CarrierId { get; set; } = PatchField<Guid?>.NotSet();
}