using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProductContent.AddProductContent;

public class AddProductContentValidation : AbstractValidator<AddProductContentCommand>
{
	public AddProductContentValidation()
	{
		RuleForEach(cmd => cmd.Contents)
			.Must((parent, kvp) => kvp.Key != parent.ParentProductId)
			.WithLocalizableError(ArticleContentSelfReferenceNotAllowedMessage.Instance);

		RuleForEach(cmd => cmd.Contents)
			.Must(kvp => kvp.Value >= 0)
			.WithLocalizableError(ArticleContentCountMustBeNonNegativeMessage.Instance);
	}
}
