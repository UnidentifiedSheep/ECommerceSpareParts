using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.EditProductReservation;

public class EditProductReservationValidation : AbstractValidator<EditProductReservationCommand>
{
	public EditProductReservationValidation()
	{
		RuleFor(z => z.NewValue.GivenPrice)
			.Must(z => Math.Round(z!.Value, 2) > 0)
			.When(z => z.NewValue.GivenPrice != null)
			.WithLocalizableError(ArticleReservationGivenPriceMustBePositiveMessage.Instance);
	}
}
