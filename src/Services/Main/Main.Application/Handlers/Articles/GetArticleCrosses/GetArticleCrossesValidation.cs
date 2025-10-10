using FluentValidation;
using Main.Application.Handlers.BaseValidators;
using AmwArticleDto = Main.Core.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleDto = Main.Core.Dtos.Member.Articles.ArticleFullDto;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public class GetArticleCrossesAmwValidation : AbstractValidator<GetArticleCrossesQuery<AmwArticleDto>>
{
    public GetArticleCrossesAmwValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}

public class GetArticleCrossesMemberValidation : AbstractValidator<GetArticleCrossesQuery<MemberArticleDto>>
{
    public GetArticleCrossesMemberValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}