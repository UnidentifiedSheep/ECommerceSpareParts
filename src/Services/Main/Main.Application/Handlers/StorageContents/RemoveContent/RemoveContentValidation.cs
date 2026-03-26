using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.StorageContents.RemoveContent;

public class RemoveContentValidation : AbstractValidator<RemoveContentCommand>
{
    public RemoveContentValidation()
    {
        RuleForEach(x => x.Content)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Value)
                    .GreaterThan(0)
                    .WithLocalizationKey("storage.content.remove.count.positive");
            });

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithLocalizationKey("storage.content.remove.list.not.empty");

        RuleFor(x => x.StorageName)
            .NotEmpty()
            .When(x => !x.TakeFromOtherStorages)
            .WithLocalizationKey("storage.content.remove.storage.required");
    }
}