using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductReservations.CreateProductReservation;

public class CreateProductReservationValidation : AbstractValidator<CreateProductReservationCommand>
{
    public CreateProductReservationValidation()
    {
        RuleFor(x => x.Reservation.ProposedPrice)
            .Must(z => !z.HasValue || Math.Round(z.Value, 2) > 0)
            .When(z => z.Reservation.ProposedPrice.HasValue)
            .WithLocalizationKey("article.reservation.given.price.must.be.positive");

        RuleFor(x => x.Reservation.ReservedCount)
            .GreaterThan(0)
            .WithLocalizationKey("article.reservation.initial.count.must.be.positive");

        RuleFor(x => x.Reservation.CurrentCount)
            .GreaterThan(0)
            .WithLocalizationKey("article.reservation.current.count.must.be.positive");

        RuleFor(x => x.Reservation.ReservedCount)
            .GreaterThanOrEqualTo(x => x.Reservation.CurrentCount)
            .WithLocalizationKey("article.reservation.initial.count.not.less.than.current");
    }
}
