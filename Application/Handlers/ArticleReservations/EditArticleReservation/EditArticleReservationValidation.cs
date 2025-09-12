using FluentValidation;

namespace Application.Handlers.ArticleReservations.EditArticleReservation;

public class EditArticleReservationValidation : AbstractValidator<EditArticleReservationCommand>
{
    public EditArticleReservationValidation()
    {
        RuleFor(z => z.NewValue.GivenPrice)
            .Must(z => Math.Round(z!.Value, 2) > 0)
            .When(z => z.NewValue.GivenPrice != null)
            .WithMessage("Предложенная цена должна быть больше 0");
        RuleFor(z => new {z.NewValue.InitialCount, z.NewValue.CurrentCount})
            .Must(z => z.InitialCount >= z.CurrentCount)
            .WithMessage("Количество которое было зарезервировано, не может быть меньше текущего.");
        RuleFor(z => z.NewValue.InitialCount)
            .GreaterThan(0)
            .WithMessage("Общее количество для резервации должно быть больше 0");
    }
}