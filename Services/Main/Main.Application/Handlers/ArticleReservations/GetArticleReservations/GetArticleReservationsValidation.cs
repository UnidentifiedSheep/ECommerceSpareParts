using FluentValidation;

namespace Main.Application.Handlers.ArticleReservations.GetArticleReservations;

public class GetArticleReservationsValidation : AbstractValidator<GetArticleReservationsQuery>
{
    public GetArticleReservationsValidation()
    {
        RuleFor(x => x.SearchTerm)
            .MinimumLength(3)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithMessage("Минимальная длинна строки поиска 3");

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");

        RuleFor(query => query.Similarity)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1)
            .When(x => x.Similarity != null)
            .WithMessage("Уровень схожести должен быть от 0 до 1");
    }
}