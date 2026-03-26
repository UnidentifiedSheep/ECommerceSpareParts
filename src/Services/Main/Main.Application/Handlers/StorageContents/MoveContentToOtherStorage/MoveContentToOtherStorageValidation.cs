using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.StorageContents.MoveContentToOtherStorage;

public class MoveContentToOtherStorageValidation : AbstractValidator<MoveContentToOtherStorageCommand>
{
    public MoveContentToOtherStorageValidation()
    {
        RuleFor(x => x.Movements)
            .Must(x =>
            {
                var all = x.ToList();
                var ids = all.Select(z => z.StorageContentId).ToHashSet();
                return all.Count == ids.Count;
            })
            .WithLocalizationKey("storage.content.move.no.duplicates");

        RuleFor(x => x.Movements)
            .NotEmpty()
            .WithLocalizationKey("storage.content.move.not.empty");
    }
}