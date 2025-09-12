using FluentValidation;

namespace Application.Handlers.ArticleReservations.CreateArticleReservation;

public class CreateArticleReservationValidation : AbstractValidator<CreateArticleReservationCommand>
{
    public CreateArticleReservationValidation()
    {
        RuleFor(x => x.Reservations.Count)
            .LessThanOrEqualTo(100)
            .WithMessage("За раз можно добавить максимум 100 резерваций.");
        RuleForEach(x => x.Reservations)
            .ChildRules(x =>
            {
                x.RuleFor(z => z.GivenPrice)
                    .Must(z => !z.HasValue || Math.Round(z.Value, 2) > 0)
                    .When(z => z.GivenPrice.HasValue)
                    .WithMessage("Предложенная цена должна быть больше 0 или не установленна");

                x.RuleFor(z => z.InitialCount)
                    .GreaterThan(0)
                    .WithMessage("Общее количество для резервации должно быть больше 0");
                
                x.RuleFor(z => z.CurrentCount)
                    .GreaterThan(0)
                    .WithMessage("Текущее количество для резервации должно быть больше 0");

                x.RuleFor(z => z.InitialCount)
                    .GreaterThanOrEqualTo(z => z.CurrentCount)
                    .WithMessage("Начальное количество не может быть меньше текущего количества");
            });
    }
}