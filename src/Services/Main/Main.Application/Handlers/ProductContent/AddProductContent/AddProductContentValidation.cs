using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ProductContent.AddProductContent;

public class AddProductContentValidation : AbstractValidator<AddProductContentCommand>
{
    public AddProductContentValidation()
    {
        RuleForEach(cmd => cmd.Contents)
            .Must((parent, kvp) => kvp.Key != parent.ParentProductId)
            .WithLocalizationKey("article.content.self.reference.not.allowed");

        RuleForEach(cmd => cmd.Contents)
            .Must(kvp => kvp.Value >= 0)
            .WithLocalizationKey("article.content.count.must.be.non.negative");
    }
}