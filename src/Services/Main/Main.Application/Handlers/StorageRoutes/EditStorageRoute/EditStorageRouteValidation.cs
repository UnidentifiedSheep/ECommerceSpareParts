using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.StorageRoutes.EditStorageRoute;

public class EditStorageRouteValidation : AbstractValidator<EditStorageRouteCommand>
{
    public EditStorageRouteValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.PatchStorageRoute.CurrencyId.Value)
            .CurrencyMustExist(currencyConverter)
            .When(x => x.PatchStorageRoute.CurrencyId.IsSet);

        RuleFor(x => x.PatchStorageRoute.PriceKg.Value)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("storage.route.price.kg.min")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("storage.route.price.kg.precision")
            .When(x => x.PatchStorageRoute.PriceKg.IsSet);

        RuleFor(x => x.PatchStorageRoute.PricePerM3.Value)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("storage.route.price.m3.min")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("storage.route.price.m3.precision")
            .When(x => x.PatchStorageRoute.PricePerM3.IsSet);

        RuleFor(x => x.PatchStorageRoute.PricePerOrder.Value)
            .GreaterThanOrEqualTo(0)
            .WithLocalizationKey("storage.route.price.order.min")
            .PrecisionScale(18, 2, true)
            .WithLocalizationKey("storage.route.price.order.precision")
            .When(x => x.PatchStorageRoute.PricePerOrder.IsSet);

        RuleFor(x => x.PatchStorageRoute.DistanceM.Value)
            .GreaterThanOrEqualTo(1)
            .WithLocalizationKey("storage.route.distance.min")
            .When(x => x.PatchStorageRoute.DistanceM.IsSet);

        RuleFor(x => x.PatchStorageRoute.DeliveryTimeMinutes.Value)
            .GreaterThanOrEqualTo(1)
            .WithLocalizationKey("storage.route.delivery.time.min")
            .When(x => x.PatchStorageRoute.DeliveryTimeMinutes.IsSet);
    }
}