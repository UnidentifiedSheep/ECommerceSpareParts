using FluentValidation;

namespace Application.Handlers.ArticleReservations.SubtractCountFromReservations;

public class SubtractCountFromReservationsValidation : AbstractValidator<SubtractCountFromReservationsCommand>
{
    public SubtractCountFromReservationsValidation()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Id пользователя не может быть пустым");

        RuleFor(x => x.WhoUpdated)
            .NotEmpty()
            .WithMessage("Id пользователя который обновляет резервации");
    }
}