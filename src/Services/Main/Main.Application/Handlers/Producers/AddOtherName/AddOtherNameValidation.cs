using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Producers.AddOtherName;

public class AddOtherNameValidation : AbstractValidator<AddOtherNameCommand>
{
    public AddOtherNameValidation()
    {
        RuleFor(x => x.OtherName)
            .NotEmpty()
            .WithLocalizationKey("producer.other.name.not.empty")
            .Must(name => name.Trim().Length >= 2)
            .WithLocalizationKey("producer.other.name.min.length")
            .Must(name => name.Trim().Length <= 64)
            .WithLocalizationKey("producer.other.name.max.length");

        RuleFor(x => x.WhereUsed)
            .Must(name => string.IsNullOrWhiteSpace(name) || name.Trim().Length <= 64)
            .WithLocalizationKey("producer.other.name.where.used.max.length");
    }
}