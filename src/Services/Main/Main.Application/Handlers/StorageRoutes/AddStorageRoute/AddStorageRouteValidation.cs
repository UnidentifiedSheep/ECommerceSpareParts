using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.StorageRoutes.AddStorageRoute;

public class AddStorageRouteValidation : AbstractValidator<AddStorageRouteCommand>
{
    public AddStorageRouteValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => new { x.StorageTo, x.StorageFrom })
            .Must(x => x.StorageTo != x.StorageFrom)
            .WithLocalizationKey("storage.route.same.storages");

        RuleFor(x => x.Distance)
            .GreaterThanOrEqualTo(1)
            .WithLocalizationKey("storage.route.distance.min");

        RuleFor(x => x.DeliveryTime)
            .GreaterThanOrEqualTo(1)
            .WithLocalizationKey("storage.route.delivery.time.min");

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);

        RuleFor(x => x.PriceKg)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("storage.route.price.kg.min")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("storage.route.price.kg.precision");

        RuleFor(x => x.PriceM3)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("storage.route.price.m3.min")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("storage.route.price.m3.precision");

        RuleFor(x => x.PricePerOrder)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("storage.route.price.order.min")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("storage.route.price.order.precision");
    }
}