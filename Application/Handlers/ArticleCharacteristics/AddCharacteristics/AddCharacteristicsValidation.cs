using FluentValidation;

namespace Application.Handlers.ArticleCharacteristics.AddCharacteristics;

public class AddCharacteristicsValidation : AbstractValidator<AddCharacteristicsCommand>
{
    public AddCharacteristicsValidation()
    {
        RuleForEach(x => x.Characteristics)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Value)
                    .NotEmpty()
                    .WithMessage("Значение не должно быть пустым");
                z.RuleFor(x => x.Value)
                    .MinimumLength(3)
                    .WithMessage("Минимальная длина значение 3 символа")
                    .MaximumLength(128)
                    .WithMessage("Длина значение не должна быть больше 128 символов");
                
                z.RuleFor(x => x.Name)
                    .MaximumLength(128)
                    .WithMessage("Название параметра не должно быть больше 128 символов");
            });
    }
}