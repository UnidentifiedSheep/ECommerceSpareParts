using FluentValidation;
using Search.Abstractions.Dtos;

namespace Search.Application.Handler.Articles.AddArticle;

public class AddArticleValidation : AbstractValidator<AddArticleCommand>
{
    public AddArticleValidation()
    {
        
    }
}