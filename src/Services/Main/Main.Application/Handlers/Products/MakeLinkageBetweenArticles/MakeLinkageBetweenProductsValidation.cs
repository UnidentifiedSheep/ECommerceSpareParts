using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Products.MakeLinkageBetweenArticles;

public class MakeLinkageBetweenProductsValidation : AbstractValidator<MakeLinkageBetweenProductsCommand>
{
	public MakeLinkageBetweenProductsValidation()
	{
		RuleForEach(x => x.Linkages)
			.ChildRules(z =>
			{
				z
					.RuleFor(x => new
					{
						x.ProductId, x.CrossProductId
					})
					.Must(x => x.ProductId != x.CrossProductId)
					.WithLocalizableError(ArticleLinkageArticleCannotEqualCrossArticleMessage.Instance);
			});
	}
}
