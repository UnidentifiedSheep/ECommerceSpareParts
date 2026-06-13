using FluentValidation;
using FluentValidation.Results;
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
                item.RuleFor(x => x.Count)
                    .GreaterThan(0)
                    .WithLocalizationKey("storage.content.count.greater.than.zero");
            });

        RuleForEach(x => x.Items)
            .Custom((item, context) =>
            {
                switch (item)
                {
                    case SubtractStorageContentItem { StorageContentId: <= 0 }:
                        context.AddFailure(CreateFailure(
                            nameof(SubtractStorageContentItem.StorageContentId),
                            "storage.content.id.greater.than.zero"));
                        break;
                    case SubtractProductFromStorageItem byProduct:
                        if (byProduct.ProductId <= 0)
                            context.AddFailure(CreateFailure(
                                nameof(SubtractProductFromStorageItem.ProductId),
                                "article.id.greater.than.zero"));

                        if (string.IsNullOrWhiteSpace(byProduct.StorageName))
                            context.AddFailure(CreateFailure(
                                nameof(SubtractProductFromStorageItem.StorageName),
                                "storage.name.not.empty"));
                        break;
                }
            });
    }

    private static ValidationFailure CreateFailure(string propertyName, string errorCode)
    {
        return new ValidationFailure(propertyName, "Validation failed")
        {
            ErrorCode = errorCode
        };
    }
}
