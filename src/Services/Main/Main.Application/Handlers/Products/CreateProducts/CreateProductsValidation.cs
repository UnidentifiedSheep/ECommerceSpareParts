using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Articles.CreateArticles;

public class CreateProductsValidation : AbstractValidator<CreateProductsCommand>
{
    public CreateProductsValidation()
    {
        RuleFor(x => x.NewProducts)
            .NotEmpty()
            .WithLocalizationKey("article.create.articles.must.have.at.least.one");

        RuleFor(x => x.NewProducts)
            .Must(x => x.Count <= 100)
            .WithLocalizationKey("article.create.articles.max.100.at.once");

        RuleForEach(x => x.NewProducts).ChildRules(content =>
        {
            content.RuleFor(x => x.Sku)
                .NotEmpty()
                .WithLocalizationKey("article.articleNumber.must.not.be.empty");
            content.RuleFor(x => x.Sku)
                .Must(x => x.Trim().Length >= 3)
                .WithLocalizationKey("article.articleNumber.min.length.3");
            content.RuleFor(x => x.Sku)
                .Must(x => x.Trim().Length <= 128)
                .WithLocalizationKey("article.articleNumber.max.length.128");

            content.RuleFor(x => x.Name)
                .NotEmpty()
                .WithLocalizationKey("article.name.must.not.be.empty");
            content.RuleFor(x => x.Name)
                .Must(x => x.Trim().Length <= 255)
                .WithLocalizationKey("article.name.max.length.255");

            content.RuleFor(x => x.Description)
                .Must(x => x?.Trim().Length <= 255)
                .When(x => x.Description != null)
                .WithLocalizationKey("article.description.max.length.255");

            content.RuleFor(x => x.Indicator)
                .Must(x => x?.Trim().Length <= 24)
                .When(x => x.Indicator != null)
                .WithLocalizationKey("article.indicator.max.length.24");
        });
    }
}