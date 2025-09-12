using FluentValidation;

namespace Application.Handlers.ArticleCharacteristics.PatchCharacteristics;

public class PatchCharacteristicsValidation : AbstractValidator<PatchCharacteristicsCommand>
{
    public PatchCharacteristicsValidation()
    {
        RuleFor(x => x.NewValues.Value.Value)
            .NotEmpty()
            .When(x => x.NewValues.Value.IsSet)
            .WithMessage("Значение не должно быть пустым");
        RuleFor(x => x.NewValues.Value.Value)
            .MinimumLength(3)
            .When(x => x.NewValues.Value.IsSet)
            .WithMessage("Минимальная длина значение 3 символа")
            .MaximumLength(128)
            .When(x => x.NewValues.Value.IsSet)
            .WithMessage("Длина значение не должна быть больше 128 символов");

        RuleFor(x => x.NewValues.Name.Value)
            .MaximumLength(128)
            .When(x => x.NewValues.Name.IsSet)
            .WithMessage("Название параметра не должно быть больше 128 символов");
    }
}