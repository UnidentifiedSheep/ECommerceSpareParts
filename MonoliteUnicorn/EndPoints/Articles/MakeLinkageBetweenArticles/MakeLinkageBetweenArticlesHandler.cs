using System.Text;
using Core.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.CacheService;

namespace MonoliteUnicorn.EndPoints.Articles.MakeLinkageBetweenArticles;

public record MakeLinkageBetweenArticlesCommand(NewArticleLinkageDto Linkage) : ICommand<Unit>;

public class MakeLinkageBetweenArticlesValidation : AbstractValidator<MakeLinkageBetweenArticlesCommand>
{
    public MakeLinkageBetweenArticlesValidation()
    {
        RuleFor(x => new { x.Linkage.ArticleId, x.Linkage.CrossArticleId })
            .Must(x => x.ArticleId != x.CrossArticleId)
            .WithMessage("Артикул не может быть таким же как кросс артикул");
    }
}
public class MakeLinkageBetweenArticlesHandler(DContext context, CacheQueue cacheQueue) : ICommandHandler<MakeLinkageBetweenArticlesCommand, Unit>
{
    public async Task<Unit> Handle(MakeLinkageBetweenArticlesCommand request, CancellationToken cancellationToken)
    {
        var linkage = request.Linkage;
        await using var dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var queryBuilder = new StringBuilder("INSERT INTO article_crosses (article_id, article_cross_id) VALUES ");
        var leftArticle = await context.Articles.AsNoTracking()
                              .FirstOrDefaultAsync(x => x.Id == linkage.ArticleId, cancellationToken) 
                          ?? throw new ArticleNotFoundException(linkage.ArticleId);
        var rightArticle = await context.Articles.AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Id == linkage.CrossArticleId, cancellationToken) 
                           ?? throw new ArticleNotFoundException(linkage.CrossArticleId);
        switch (linkage.LinkageType)
        {
            case ArticleLinkageTypes.SingleCross:
            {
                queryBuilder.Append($" ({leftArticle.Id}, {rightArticle.Id}), ({rightArticle.Id}, {leftArticle.Id})");
                break;
            }

            case ArticleLinkageTypes.FullCross:
            {
                var leftIds = await context.ArticleCrosses
                    .AsNoTracking()
                    .Where(x => x.ArticleId == linkage.ArticleId || x.ArticleCrossId == linkage.ArticleId)
                    .Select(x => x.ArticleId)
                    .Union(
                        context.ArticleCrosses
                            .Where(x => x.ArticleId == linkage.ArticleId || x.ArticleCrossId == linkage.ArticleId)
                            .Select(x => x.ArticleCrossId)
                    )
                    .Distinct()
                    .ToHashSetAsync(cancellationToken);

                var rightIds = await context.ArticleCrosses
                    .AsNoTracking()
                    .Where(x => x.ArticleId == linkage.CrossArticleId || x.ArticleCrossId == linkage.CrossArticleId)
                    .Select(x => x.ArticleId)
                    .Union(
                        context.ArticleCrosses
                            .Where(x => x.ArticleId == linkage.CrossArticleId || x.ArticleCrossId == linkage.CrossArticleId)
                            .Select(x => x.ArticleCrossId)
                    )
                    .Distinct()
                    .ToHashSetAsync(cancellationToken);

                var pairs = leftIds.SelectMany(_ => rightIds, (l, r) => (l, r)).ToHashSet();
                pairs.UnionWith(leftIds.SelectMany(_ => rightIds, (l, r) => (r, l)));
                foreach (var (l, r) in pairs)
                    queryBuilder.Append($" ({l}, {r}),");

                queryBuilder.Length--;
                break;
            }

            case ArticleLinkageTypes.FullLeftToRightCross:
            {
                var leftIds = await context.ArticleCrosses
                    .AsNoTracking()
                    .Where(x => x.ArticleId == linkage.ArticleId || x.ArticleCrossId == linkage.ArticleId)
                    .Select(x => x.ArticleId)
                    .Union(
                        context.ArticleCrosses
                            .Where(x => x.ArticleId == linkage.ArticleId || x.ArticleCrossId == linkage.ArticleId)
                            .Select(x => x.ArticleCrossId)
                    )
                    .Distinct()
                    .ToHashSetAsync(cancellationToken);

                foreach (var leftId in leftIds)
                    queryBuilder.Append($" ({leftId}, {rightArticle.Id}), ({rightArticle.Id}, {leftId}),");

                queryBuilder.Length--;
                break;
            }

            case ArticleLinkageTypes.FullRightToLeftCross:
            {
                var rightIds = await context.ArticleCrosses
                    .AsNoTracking()
                    .Where(x => x.ArticleId == linkage.CrossArticleId || x.ArticleCrossId == linkage.CrossArticleId)
                    .Select(x => x.ArticleId)
                    .Union(
                        context.ArticleCrosses
                            .Where(x => x.ArticleId == linkage.CrossArticleId || x.ArticleCrossId == linkage.CrossArticleId)
                            .Select(x => x.ArticleCrossId)
                    )
                    .Distinct()
                    .ToHashSetAsync(cancellationToken);

                foreach (var rightId in rightIds)
                    queryBuilder.Append($" ({rightId}, {leftArticle.Id}), ({leftArticle.Id}, {rightId}),");

                queryBuilder.Length--;
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        
        queryBuilder.Append($" ON CONFLICT DO NOTHING");
        await context.Database.ExecuteSqlRawAsync(queryBuilder.ToString(), cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);
        
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.CacheArticleFromZeroIfExistAsync(request.Linkage.ArticleId);
            await cache.CacheArticleFromZeroIfExistAsync(request.Linkage.CrossArticleId);
        });
        
        return Unit.Value;
    }
}