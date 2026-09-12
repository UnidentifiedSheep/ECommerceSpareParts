using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.CreateProductReservation;

public class CreateProductReservationValidation : AbstractValidator<CreateProductReservationCommand>
{
	public CreateProductReservationValidation()
	{
		RuleFor(x => x.Reservation.OrganizationId)
			.NotEmpty()
			.WithLocalizableError(ArticleReservationOrganizationIdMustNotBeEmptyMessage.Instance);

		RuleFor(x => x.Reservation.ProposedPrice)
			.Must(z => !z.HasValue || Math.Round(z.Value, 2) > 0)
			.When(z => z.Reservation.ProposedPrice.HasValue)
			.WithLocalizableError(ArticleReservationGivenPriceMustBePositiveMessage.Instance);

		RuleFor(x => x.Reservation.ReservedCount)
			.GreaterThan(0)
			.WithLocalizableError(ArticleReservationInitialCountMustBePositiveMessage.Instance);

		RuleFor(x => x.Reservation.CurrentCount)
			.GreaterThanOrEqualTo(0)
			.WithLocalizableError(ArticleReservationCurrentCountMustBePositiveMessage.Instance);

		RuleFor(x => x.Reservation.ReservedCount)
			.GreaterThanOrEqualTo(x => x.Reservation.CurrentCount)
			.WithLocalizableError(ArticleReservationInitialCountNotLessThanCurrentMessage.Instance);
	}
}
