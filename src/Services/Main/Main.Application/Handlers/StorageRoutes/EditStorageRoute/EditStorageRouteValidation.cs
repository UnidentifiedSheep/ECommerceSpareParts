using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;

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
            .WithMessage("Минимальная цена за кг 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Цена за кг может содержать не более двух знаков после запятой")
            .When(x => x.PatchStorageRoute.PriceKg.IsSet);
        
        RuleFor(x => x.PatchStorageRoute.PricePerM3.Value)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Минимальная цена за м^3 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Цена за м^3 может содержать не более двух знаков после запятой")
            .When(x => x.PatchStorageRoute.PricePerM3.IsSet);
        
        RuleFor(x => x.PatchStorageRoute.PricePerOrder.Value)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Минимальная цена за заказ 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Цена за заказ может содержать не более двух знаков после запятой")
            .When(x => x.PatchStorageRoute.PricePerOrder.IsSet);
        
        RuleFor(x => x.PatchStorageRoute.DistanceM.Value)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Минимальная дистанция маршрута - 1 метр.")
            .When(x => x.PatchStorageRoute.DistanceM.IsSet);

        RuleFor(x => x.PatchStorageRoute.DeliveryTimeMinutes.Value)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Минимальное время доставки - 1 минута")
            .When(x => x.PatchStorageRoute.DeliveryTimeMinutes.IsSet);
    }
}