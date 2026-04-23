using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Products.PatchProduct;

public class PatchProductValidation : AbstractValidator<PatchArticleCommand>
{
    public PatchProductValidation()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        // ArticleNumber
        RuleFor(x => x.PatchProduct.Sku.Value)
            .NotEmpty()
            .When(x => x.PatchProduct.Sku.IsSet)
            .WithLocalizationKey("article.articleNumber.must.not.be.empty");
        RuleFor(x => x.PatchProduct.Sku.Value)
            .Must(x => x != null && x.Trim().Length >= 3)
            .When(x => x.PatchProduct.Sku.IsSet)
            .WithLocalizationKey("article.articleNumber.min.length.3");
        RuleFor(x => x.PatchProduct.Sku.Value)
            .Must(x => x != null && x.Trim().Length <= 128)
            .When(x => x.PatchProduct.Sku.IsSet)
            .WithLocalizationKey("article.articleNumber.max.length.128");

        // ArticleName
        RuleFor(x => x.PatchProduct.Name.Value)
            .NotEmpty()
            .When(x => x.PatchProduct.Name.IsSet)
            .WithLocalizationKey("article.name.must.not.be.empty");
        RuleFor(x => x.PatchProduct.Name.Value)
            .Must(x => x?.Trim().Length >= 3)
            .When(x => x.PatchProduct.Name.IsSet)
            .WithLocalizationKey("article.name.min.length.3");
        RuleFor(x => x.PatchProduct.Name.Value)
            .Must(x => x?.Trim().Length <= 255)
            .When(x => x.PatchProduct.Name.IsSet)
            .WithLocalizationKey("article.name.max.length.255");
    }
}