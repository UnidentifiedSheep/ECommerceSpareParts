using Core.Interface;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.CreateArticle;

public record CreateArticleCommand(List<NewArticleDto> NewArticles) : ICommand;

public class CreateArticleValidation : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleValidation()
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
                .WithMessage("Максимальная длина описания 255 символов");
            
            content.RuleFor(x => x.Indicator)
                .Must(x => x?.Trim().Length <= 24)
                .WithMessage("Максимальная длина индикатора/цвета 24 символа");
        });
    }
}
public class CreateArticleHandler(DContext context) : ICommandHandler<CreateArticleCommand>
{
    public async Task<Unit> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        var articles = request.NewArticles.Adapt<List<Article>>();
        var producersIds = articles
            .Select(x => x.ProducerId)
            .Distinct()
            .ToList();
        var producers = await context.Producers
            .AsNoTracking()
            .Where(x => producersIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        if (producers.Count != producersIds.Count)
        {
            var notFoundIds = producersIds.Except(producers.Keys);
            throw new ProducerNotFoundException(notFoundIds);
        }
        
        await context.Articles.AddRangeAsync(articles, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}