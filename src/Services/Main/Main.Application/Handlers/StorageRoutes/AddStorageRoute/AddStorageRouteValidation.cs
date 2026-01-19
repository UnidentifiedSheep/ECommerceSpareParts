using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;

namespace Main.Application.Handlers.StorageRoutes.AddStorageRoute;

public class AddStorageRouteValidation : AbstractValidator<AddStorageRouteCommand>
{
    public AddStorageRouteValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => new { x.StorageTo, x.StorageFrom })
            .Must(x => x.StorageTo != x.StorageFrom)
            .WithMessage("Склад отправления и склад назначения не могут быть одинаковыми.");
        
        RuleFor(x => x.Distance)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Минимальная дистанция маршрута - 1 метр.");

        RuleFor(x => x.DeliveryTime)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Минимальное время доставки - 1 минута");

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);

        RuleFor(x => x.PriceKg)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Минимальная цена за кг 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Цена за кг может содержать не более двух знаков после запятой");
        
        RuleFor(x => x.PriceM3)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Минимальная цена за м^3 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Цена за м^3 может содержать не более двух знаков после запятой");
        
        RuleFor(x => x.PricePerOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Минимальная цена за заказ 0.")
            .PrecisionScale(18, 2, true)
            .WithMessage("Цена за заказ может содержать не более двух знаков после запятой");
    }
}