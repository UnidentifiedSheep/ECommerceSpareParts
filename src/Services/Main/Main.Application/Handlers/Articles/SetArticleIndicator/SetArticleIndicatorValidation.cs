using FluentValidation;

namespace Main.Application.Handlers.Articles.SetArticleIndicator;

public class SetArticleIndicatorValidation : AbstractValidator<SetArticleIndicatorCommand>
{
    public SetArticleIndicatorValidation()
    {
        RuleFor(x => x.Indicator)
            .MaximumLength(24)
            .WithMessage("Максимальная длина индикатора составляет 24 символа");
    }
}