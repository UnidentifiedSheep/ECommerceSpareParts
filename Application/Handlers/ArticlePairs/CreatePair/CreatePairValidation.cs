using FluentValidation;

namespace Application.Handlers.ArticlePairs.CreatePair;

public class CreatePairValidation : AbstractValidator<CreatePairCommand>
{
    public CreatePairValidation()
    {
        RuleFor(x => new { x.LeftArticleId, x.RightArticleId })
            .Must(x => x.LeftArticleId != x.RightArticleId)
            .WithMessage("Артикул не может быть парой самому себе");
    }
}