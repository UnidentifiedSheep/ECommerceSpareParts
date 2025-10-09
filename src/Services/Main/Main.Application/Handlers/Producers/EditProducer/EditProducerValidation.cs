using FluentValidation;

namespace Main.Application.Handlers.Producers.EditProducer;

public class EditProducerValidation : AbstractValidator<EditProducerCommand>
{
    public EditProducerValidation()
    {
        RuleFor(x => x.EditProducer.Description)
            .Must(desc => desc.Value?.Trim().Length <= 500)
            .When(x => x.EditProducer.Description.IsSet)
            .WithMessage("Максимальная длина описания — 500 символов");

        RuleFor(x => x.EditProducer.Name.Value)
            .NotEmpty()
            .WithMessage("Название производителя не может быть пустым")
            .MinimumLength(2)
            .WithMessage("Минимальная длина названия производителя — 2 символа")
            .MaximumLength(64)
            .WithMessage("Максимальная длина названия производителя — 64 символа")
            .When(x => x.EditProducer.Name.IsSet);
    }
}