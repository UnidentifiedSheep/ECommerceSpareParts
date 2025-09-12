using FluentValidation;

namespace Application.Handlers.Articles.CreateArticles;

public class CreateArticlesValidation : AbstractValidator<CreateArticlesCommand>
{
    public CreateArticlesValidation()
    {
        RuleFor(x => x.NewArticles)
            .NotEmpty()
            .WithMessage("Должен быть указан хотя бы один артикул на добавление");
        RuleFor(x => x.NewArticles)
            .Must(x => x.Count <= 100)
            .WithMessage("Максимум можно добавить 100 артикулов за раз");
        RuleForEach(x => x.NewArticles).ChildRules(content =>
        {
            content.RuleFor(x => x.ArticleNumber).NotEmpty()
                .WithMessage("Артикул не может быть пустым");
            content.RuleFor(x => x.ArticleNumber)
                .Must(x => x.Trim().Length <= 128)
                .WithMessage("Максимальная длина артикула 128 символов");
            content.RuleFor(x => x.ArticleNumber)
                .Must(x => x.Trim().Length >= 3)
                .WithMessage("Минимальная длина артикула 3 символа");
            
            content.RuleFor(x => x.Name).NotEmpty()
                .WithMessage("Название артикула не может быть пустым");
            content.RuleFor(x => x.Name)
                .Must(x => x.Trim().Length <= 255)
                .WithMessage("Максимальная длина названия 255 символов");
            
            content.RuleFor(x => x.Description)
                .Must(x => x?.Trim().Length <= 255)
                .When(x => x.Description != null)
                .WithMessage("Максимальная длина описания 255 символов");
            
            content.RuleFor(x => x.Indicator)
                .Must(x => x?.Trim().Length <= 24)
                .When(x => x.Indicator != null)
                .WithMessage("Максимальная длина индикатора/цвета 24 символа");
        });
    }
}