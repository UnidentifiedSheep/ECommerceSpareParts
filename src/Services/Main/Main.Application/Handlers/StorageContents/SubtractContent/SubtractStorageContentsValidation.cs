using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.StorageContents.SubtractContent;

public class SubtractStorageContentsValidation : AbstractValidator<SubtractStorageContentsCommand>
{
    public SubtractStorageContentsValidation()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithLocalizationKey("storage.content.items.required");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.StorageContentId)
                    .GreaterThan(0)
                    .WithLocalizationKey("storage.content.id.greater.than.zero");

                item.RuleFor(x => x.Count)
                    .GreaterThan(0)
                    .WithLocalizationKey("storage.content.count.greater.than.zero");
            });
    }
}
