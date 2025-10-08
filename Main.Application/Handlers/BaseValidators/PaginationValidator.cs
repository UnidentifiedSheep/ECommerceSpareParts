using Core.Models;
using FluentValidation;

namespace Main.Application.Handlers.BaseValidators;

public class PaginationValidator : AbstractValidator<PaginationModel>
{
    public PaginationValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}