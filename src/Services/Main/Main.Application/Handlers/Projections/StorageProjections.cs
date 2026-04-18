using System.Linq.Expressions;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Dtos.Currencies;
using Main.Entities.Storage;

namespace Main.Application.Handlers.Currencies.Projections;

public static class StorageProjections
{
    public static Expression<Func<StorageRoute, StorageRouteDto>> StorageRouteProjection =
        x => new StorageRouteDto 
        {
            Id = x.Id,
            CarrierId = x.CarrierId,
            Currency = new CurrencyDto
            {
                Code = x.Currency.Code,
                Name = x.Currency.Name,
                CurrencySign = x.Currency.CurrencySign,
                Id =  x.Currency.Id,
                ToUsdRate = x.Currency.CurrencyToUsd == null ? 0 : x.Currency.CurrencyToUsd.ToUsd,
                ShortName = x.Currency.ShortName,
            },
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
}