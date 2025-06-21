using System.Text;
using Core.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.ChangeArticleLinkage;

public record ChangeArticleLinkageCommand(IEnumerable<NewArticleLinkageDto> Linkages) : ICommand<Unit>;

public class ChangeArticleLinkageHandler(DContext context) : ICommandHandler<ChangeArticleLinkageCommand, Unit>
{
    public async Task<Unit> Handle(ChangeArticleLinkageCommand request, CancellationToken cancellationToken)
    {
        await using var dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var firstWithError = request.Linkages
            .FirstOrDefault(x => x.ArticleId == x.CrossArticleId);
        if (firstWithError != null)
            throw new LinkageCrossArticleIdException(firstWithError.CrossArticleId);
        foreach (var linkage in request.Linkages)
        {
            var queryBuilder = new StringBuilder("INSERT INTO article_crosses (article_id, article_cross_id) VALUES ");
            var leftArticle = await context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == linkage.ArticleId, cancellationToken)
                ?? throw new ArticleNotFoundException(linkage.ArticleId);
            var rightArticle = await context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == linkage.CrossArticleId, cancellationToken)
                ?? throw new ArticleNotFoundException(linkage.CrossArticleId);
            switch (linkage.LinkageType)
            {
                case ArticleLinkageTypes.SingleCross:
                    queryBuilder.Append($" ({leftArticle.Id}, {rightArticle.Id}), ({rightArticle.Id}, {leftArticle.Id})");
                    break;
                case ArticleLinkageTypes.FullCross:
                    var leftFull = await context.ArticleCrosses
                        .AsNoTracking()
                        .Where(x => x.ArticleId == linkage.ArticleId || x.ArticleCrossId == linkage.ArticleId)
                        .ToListAsync(cancellationToken);
                    var left = leftFull.SelectMany(x => new[] { x.ArticleId, x.ArticleCrossId })
                        .Append(linkage.ArticleId).DistinctBy(x => x);
                    var rightFull = await context.ArticleCrosses
                        .AsNoTracking()
                        .Where(x => x.ArticleId == linkage.CrossArticleId || x.ArticleCrossId == linkage.CrossArticleId)
                        .ToListAsync(cancellationToken);
                    var right = rightFull.SelectMany(x => new[] { x.ArticleId, x.ArticleCrossId })
                        .Append(linkage.CrossArticleId).DistinctBy(x => x);
                    var joined = left
                        .SelectMany(leftItem => right, (leftItem, rightItem) => $"({leftItem}, {rightItem})")
                        .ToList();
                    queryBuilder.Append(string.Join(", ", joined));
                    break;
                //TODO: ДОПИСАТЬ СОЗДАНИЕ ЛИНКОВКИ
                case ArticleLinkageTypes.FullLeftToRightCross:
                    break;
                case ArticleLinkageTypes.FullRightToLeftCross:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            queryBuilder.Append($" ON CONFLICT DO NOTHING");
            await context.Database.ExecuteSqlRawAsync(queryBuilder.ToString(), cancellationToken);
        }
        await dbTransaction.CommitAsync(cancellationToken);
        return Unit.Value;
    }
}