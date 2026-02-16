using Application.Common.Aot.Interfaces;
using Sannr;
using Search.Abstractions.Dtos;

namespace Search.Application.Handler.Articles.AddArticle;

public class AddArticleValidation : IValidation<AddArticleCommand>
{
    public Task<ValidationResult> ValidateAsync(AddArticleCommand request)
    {
        return ArticleDto.ValidateAsync(request.Article);
    }
}