using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductReservations.EditProductReservation;

public class EditProductReservationValidation : AbstractValidator<EditProductReservationCommand>
{
    public EditProductReservationValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(z => z.NewValue.GivenPrice)
            .Must(z => Math.Round(z!.Value, 2) > 0)
            .When(z => z.NewValue.GivenPrice != null)
            .WithLocalizationKey("article.reservation.given.price.must.be.positive");

        RuleFor(x => x.NewValue.GivenCurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}