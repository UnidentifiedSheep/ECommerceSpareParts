using System.Linq.Expressions;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Dtos.Currencies;
using Main.Entities.Storage;
using LinqKit;

namespace Main.Application.Handlers.Currencies.Projections;

public static class StorageProjections
{
    public static readonly Expression<Func<StorageRoute, StorageRouteDto>> StorageRouteProjection =
        x => new StorageRouteDto 
        {
            Id = x.Id,
            CarrierId = x.CarrierId,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency),
            DeliveryTimeMinutes = x.DeliveryTimeMinutes,
            DistanceM = x.DistanceM,
            FromStorageName = x.FromStorageName,
            IsActive = x.IsActive,
            MinimumPrice = x.MinimumPrice,
            PricePerKg = x.PriceKg,
            PricePerM3 = x.PricePerM3,
            PricePerOrder = x.PricePerOrder,
            PricingModel = x.PricingModel,
            RouteType = x.RouteType,
            ToStorageName = x.ToStorageName,
        };
    
    public static readonly Expression<Func<Storage, StorageDto>> StorageProjection =
        x => new StorageDto 
        {
            Name = x.Name,
            Location = x.Location,
            Description = x.Description,
            Type = x.Type,
        };
}