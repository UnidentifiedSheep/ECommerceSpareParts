using FluentValidation;

namespace Main.Application.Handlers.ArticleWeight.SetArticleWeight;

public class SetArticleWeightValidation : AbstractValidator<SetArticleWeightCommand>
{
    public SetArticleWeightValidation()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Вес должен быть больше 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Вес может содержать не более двух знаков после запятой");
    }
}