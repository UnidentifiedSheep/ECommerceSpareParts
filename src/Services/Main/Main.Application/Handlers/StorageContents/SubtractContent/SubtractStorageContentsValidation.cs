using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.StorageContents.SubtractContent;

public class SubtractStorageContentsValidation : AbstractValidator<SubtractStorageContentsCommand>
{
    public SubtractStorageContentsValidation()
    {
        RuleFor(x => x.StorageContentId)
            .GreaterThan(0)
            .WithLocalizationKey("storage.content.id.greater.than.zero");

        RuleFor(x => x.Count)
            .GreaterThan(0)
            .WithLocalizationKey("storage.content.count.greater.than.zero");
    }
}