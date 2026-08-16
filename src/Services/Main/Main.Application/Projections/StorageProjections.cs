using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Storage;
using Main.Entities.Currency;
using Main.Entities.Storage;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class StorageRouteDtoProjectionProvider
    : ProjectionProviderBase<StorageRoute, StorageRouteDto>
{
    public StorageRouteDtoProjectionProvider(
        IProjectionProvider<Currency, CurrencyDto> currencyProjection)
    {
        var currencyToDto = currencyProjection.Projection;

        Projection = x => new StorageRouteDto
        {
            Id = x.Id,
            CarrierId = x.CarrierId,
            Currency = currencyToDto.Invoke(x.Currency),
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
            ToStorageName = x.ToStorageName
        };
    }

    public override Expression<Func<StorageRoute, StorageRouteDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class StorageDtoProjectionProvider
    : ProjectionProviderBase<Storage, StorageDto>
{
    public override Expression<Func<Storage, StorageDto>> Projection { get; } =
        x => new StorageDto
        {
            Name = x.Name,
            Location = x.Location,
            Description = x.Description,
            Type = x.Type
        };
}
