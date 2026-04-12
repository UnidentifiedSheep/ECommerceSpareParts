using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.ArticlePairs.CreatePair;

public class CreatePairValidation : AbstractValidator<CreatePairCommand>
{
    public CreatePairValidation()
    {
        RuleFor(x => x)
            .Must(x => x.LeftArticleId != x.RightArticleId)
            .WithLocalizationKey("article.pair.self.reference.not.allowed");
    }
}