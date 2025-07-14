using Core.Interface;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.CacheService;

namespace MonoliteUnicorn.EndPoints.Articles.EditArticle;

public record EditArticleCommand(int ArticleId, PatchArticleDto PatchArticle) : ICommand<Unit>;

public class EditArticleValidation : AbstractValidator<EditArticleCommand>
{
    public EditArticleValidation()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        //ArticleNumber
        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .NotEmpty()
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithMessage("Артикул не должен быть пустым");

        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .Must(x => x != null && x.Trim().Length >= 3)
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithMessage("Минимальная длина артикула 3 символа");

        RuleFor(x => x.PatchArticle.ArticleNumber.Value)
            .Must(x => x != null && x.Trim().Length <= 128)
            .When(x => x.PatchArticle.ArticleNumber.IsSet)
            .WithMessage("Максимальная длина артикула 128 символов");
        
        //ArticleName
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .NotEmpty()
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithMessage("Название артикула не должен быть пустым");
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .Must(x => x?.Trim().Length > 3)
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithMessage("Минимальная длина название артикула 3 символа");
        RuleFor(x => x.PatchArticle.ArticleName.Value)
            .Must(x => x?.Trim().Length <= 255)
            .When(x => x.PatchArticle.ArticleName.IsSet)
            .WithMessage("Максимальная длина названия артикула 255 символов");
    }
}
public class EditArticleHandler(DContext context, CacheQueue cacheQueue) : ICommandHandler<EditArticleCommand, Unit>
{
    public async Task<Unit> Handle(EditArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await context.Articles
            .FirstOrDefaultAsync(x => x.Id == request.ArticleId, cancellationToken) ?? throw new ArticleNotFoundException(request.ArticleId);
        request.PatchArticle.Adapt(article);
        await context.SaveChangesAsync(cancellationToken);
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync([request.ArticleId]);
        });
        return Unit.Value;
    }
}