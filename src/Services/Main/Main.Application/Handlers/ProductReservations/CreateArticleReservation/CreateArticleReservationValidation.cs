using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticleReservations.CreateArticleReservation;

public class CreateArticleReservationValidation : AbstractValidator<CreateArticleReservationCommand>
{
    public CreateArticleReservationValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.Reservations.Count)
            .LessThanOrEqualTo(100)
            .WithLocalizationKey("article.reservation.max.count.exceeded");

        RuleForEach(x => x.Reservations)
            .ChildRules(x =>
            {
                x.RuleFor(z => z.GivenPrice)
                    .Must(z => !z.HasValue || Math.Round(z.Value, 2) > 0)
                    .When(z => z.GivenPrice.HasValue)
                    .WithLocalizationKey("article.reservation.given.price.must.be.positive");

                x.RuleFor(z => z.InitialCount)
                    .GreaterThan(0)
                    .WithLocalizationKey("article.reservation.initial.count.must.be.positive");

                x.RuleFor(z => z.CurrentCount)
                    .GreaterThan(0)
                    .WithLocalizationKey("article.reservation.current.count.must.be.positive");

                x.RuleFor(z => z.InitialCount)
                    .GreaterThanOrEqualTo(z => z.CurrentCount)
                    .WithLocalizationKey("article.reservation.initial.count.not.less.than.current");

                x.RuleFor(z => z.GivenCurrencyId)
                    .CurrencyMustExist(currencyConverter);
            });
    }
}