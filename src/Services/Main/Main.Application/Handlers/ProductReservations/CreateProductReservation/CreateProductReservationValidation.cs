using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticleReservations.CreateArticleReservation;

public class CreateProductReservationValidation : AbstractValidator<CreateProductReservationCommand>
{
    public CreateProductReservationValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.Reservations.Count)
            .LessThanOrEqualTo(100)
            .WithLocalizationKey("article.reservation.max.count.exceeded");

        RuleForEach(x => x.Reservations)
            .ChildRules(x =>
            {
                x.RuleFor(z => z.ProposedPrice)
                    .Must(z => !z.HasValue || Math.Round(z.Value, 2) > 0)
                    .When(z => z.ProposedPrice.HasValue)
                    .WithLocalizationKey("article.reservation.given.price.must.be.positive");

                x.RuleFor(z => z.ReservedCount)
                    .GreaterThan(0)
                    .WithLocalizationKey("article.reservation.initial.count.must.be.positive");

                x.RuleFor(z => z.CurrentCount)
                    .GreaterThan(0)
                    .WithLocalizationKey("article.reservation.current.count.must.be.positive");

                x.RuleFor(z => z.ReservedCount)
                    .GreaterThanOrEqualTo(z => z.CurrentCount)
                    .WithLocalizationKey("article.reservation.initial.count.not.less.than.current");

                x.RuleFor(z => z.GivenCurrencyId)
                    .CurrencyMustExist(currencyConverter);
            });
    }
}