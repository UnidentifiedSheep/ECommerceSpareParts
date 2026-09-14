using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Products.CreateProducts;

public class CreateProductsValidation : AbstractValidator<CreateProductsCommand>
{
	public const int MaxProductsPerRequest = 100;

	public CreateProductsValidation()
	{
		RuleFor(x => x.NewProducts)
			.NotEmpty()
			.WithLocalizableError(ArticleCreateArticlesMustHaveAtLeastOneMessage.Instance);

		RuleFor(x => x.NewProducts)
			.Must(x => x.Count <= MaxProductsPerRequest)
			.WithLocalizableError(ArticleCreateArticlesMax100AtOnceMessage.Instance);

		RuleForEach(x => x.NewProducts)
			.ChildRules(content =>
			{
				content
					.RuleFor(x => x.Sku)
					.NotEmpty()
					.WithLocalizableError(ArticleArticleNumberMustNotBeEmptyMessage.Instance);
				content
					.RuleFor(x => x.Sku)
					.Must(x => x.Trim().Length >= 3)
					.WithLocalizableError(ArticleArticleNumberMinLength3Message.Instance);
				content
					.RuleFor(x => x.Sku)
					.Must(x => x.Trim().Length <= 128)
					.WithLocalizableError(ArticleArticleNumberMaxLength128Message.Instance);

				content.RuleFor(x => x.Name).NotEmpty().WithLocalizableError(ArticleNameMustNotBeEmptyMessage.Instance);
				content
					.RuleFor(x => x.Name)
					.Must(x => x.Trim().Length <= 255)
					.WithLocalizableError(ArticleNameMaxLength255Message.Instance);

				content
					.RuleFor(x => x.Description)
					.Must(x => x?.Trim().Length <= 255)
					.When(x => x.Description != null)
					.WithLocalizableError(ArticleDescriptionMaxLength255Message.Instance);

				content
					.RuleFor(x => x.Indicator)
					.Must(x => x?.Trim().Length <= 24)
					.When(x => x.Indicator != null)
					.WithLocalizableError(ArticleIndicatorMaxLength24Message.Instance);
			});
	}
}
