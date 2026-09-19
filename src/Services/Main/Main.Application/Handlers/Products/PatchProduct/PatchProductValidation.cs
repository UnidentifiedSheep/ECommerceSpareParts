using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Products.PatchProduct;

public class PatchProductValidation : AbstractValidator<PatchProductCommand>
{
	public PatchProductValidation()
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		// ArticleNumber
		RuleFor(x => x.PatchProduct.Sku.Value)
			.NotEmpty()
			.When(x => x.PatchProduct.Sku.IsSet)
			.WithLocalizableError(ArticleArticleNumberMustNotBeEmptyMessage.Instance);
		RuleFor(x => x.PatchProduct.Sku.Value)
			.Must(x => x != null && x.Trim().Length >= 3)
			.When(x => x.PatchProduct.Sku.IsSet)
			.WithLocalizableError(ArticleArticleNumberMinLength3Message.Instance);
		RuleFor(x => x.PatchProduct.Sku.Value)
			.Must(x => x != null && x.Trim().Length <= 128)
			.When(x => x.PatchProduct.Sku.IsSet)
			.WithLocalizableError(ArticleArticleNumberMaxLength128Message.Instance);

		// ArticleName
		RuleFor(x => x.PatchProduct.Name.Value)
			.NotEmpty()
			.When(x => x.PatchProduct.Name.IsSet)
			.WithLocalizableError(ArticleNameMustNotBeEmptyMessage.Instance);
		RuleFor(x => x.PatchProduct.Name.Value)
			.Must(x => x?.Trim().Length >= 3)
			.When(x => x.PatchProduct.Name.IsSet)
			.WithLocalizableError(ArticleNameMinLength3Message.Instance);
		RuleFor(x => x.PatchProduct.Name.Value)
			.Must(x => x?.Trim().Length <= 255)
			.When(x => x.PatchProduct.Name.IsSet)
			.WithLocalizableError(ArticleNameMaxLength255Message.Instance);
	}
}
