using FluentValidation;

namespace Main.Application.Handlers.ArticleSizes.SetArticleSizes;

public class SetArticleSizesValidation : AbstractValidator<SetArticleSizesCommand>
{
    public SetArticleSizesValidation()
    {
        RuleFor(x => x.Height)
            .GreaterThan(0)
            .WithMessage("Высота должна быть больше 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Высота может содержать не более двух знаков после запятой");
        
        RuleFor(x => x.Width)
            .GreaterThan(0)
            .WithMessage("Ширина должна быть больше 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Ширина может содержать не более двух знаков после запятой");
        
        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Длина должна быть больше 0")
            .PrecisionScale(18, 2, true)
            .WithMessage("Длина может содержать не более двух знаков после запятой");
    }
}