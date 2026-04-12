using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticleReservations.EditArticleReservation;

public class EditArticleReservationValidation : AbstractValidator<EditArticleReservationCommand>
{
    public EditArticleReservationValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(z => z.NewValue.GivenPrice)
            .Must(z => Math.Round(z!.Value, 2) > 0)
            .When(z => z.NewValue.GivenPrice != null)
            .WithLocalizationKey("article.reservation.given.price.must.be.positive");

        RuleFor(z => new { z.NewValue.InitialCount, z.NewValue.CurrentCount })
            .Must(z => z.InitialCount >= z.CurrentCount)
            .WithLocalizationKey("article.reservation.initial.count.not.less.than.current");

        RuleFor(z => z.NewValue.InitialCount)
            .GreaterThan(0)
            .WithLocalizationKey("article.reservation.initial.count.must.be.positive");

        RuleFor(x => x.NewValue.GivenCurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}