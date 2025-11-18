using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Application.Notifications;
using Main.Core.Dtos.Amw.Articles;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Articles.MakeLinkageBetweenArticles;

[Transactional]
public record MakeLinkageBetweenArticlesCommand(NewArticleLinkageDto Linkage) : ICommand<Unit>;

public class MakeLinkageBetweenArticlesHandler(
    IMediator mediator,
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MakeLinkageBetweenArticlesCommand, Unit>
{
    public async Task<Unit> Handle(MakeLinkageBetweenArticlesCommand request, CancellationToken cancellationToken)
    {
        var linkage = request.Linkage;
        var lrArticles = await articlesRepository
            .GetArticlesByIds([linkage.ArticleId, linkage.CrossArticleId], false, cancellationToken);

        var leftArticle = lrArticles.FirstOrDefault(x => x.Id == linkage.ArticleId) ??
                          throw new ArticleNotFoundException(linkage.ArticleId);
        var rightArticle = lrArticles.FirstOrDefault(x => x.Id == linkage.CrossArticleId) ??
                           throw new ArticleNotFoundException(linkage.CrossArticleId);

        var toAdd = new HashSet<(int, int)>();

        switch (linkage.LinkageType)
        {
            case ArticleLinkageTypes.SingleCross:
                AddBidirectionalPairs(toAdd, [leftArticle.Id], [rightArticle.Id]);
                break;

            case ArticleLinkageTypes.FullCross:
                var leftIds = await GetCrossIds(linkage.ArticleId, cancellationToken);
                var rightIds = await GetCrossIds(linkage.CrossArticleId, cancellationToken);
                AddBidirectionalPairs(toAdd, leftIds, rightIds);
                break;

            case ArticleLinkageTypes.FullLeftToRightCross:
                var leftCrossIds = await GetCrossIds(linkage.ArticleId, cancellationToken);
                AddBidirectionalPairs(toAdd, leftCrossIds, rightArticle.Id);
                break;

            case ArticleLinkageTypes.FullRightToLeftCross:
                var rightCrossIds = await GetCrossIds(linkage.CrossArticleId, cancellationToken);
                AddBidirectionalPairs(toAdd, rightCrossIds, leftArticle.Id);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        await articlesRepository.AddArticleLinkage(toAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticleUpdatedNotification(linkage.ArticleId), cancellationToken);
        await mediator.Publish(new ArticleUpdatedNotification(linkage.CrossArticleId), cancellationToken);

        return Unit.Value;
    }

    private async Task<HashSet<int>> GetCrossIds(int articleId, CancellationToken ct)
    {
        return (await articlesRepository.GetArticleCrossIds(articleId, ct)).ToHashSet();
    }

    private void AddBidirectionalPairs(HashSet<(int, int)> set, IEnumerable<int> leftIds, IEnumerable<int> rightIds)
    {
        var rIds = rightIds.ToList();
        foreach (var l in leftIds)
            foreach (var r in rIds)
            {
                set.Add((l, r));
                set.Add((r, l));
            }
    }

    private void AddBidirectionalPairs(HashSet<(int, int)> set, IEnumerable<int> ids, int singleId)
    {
        foreach (var id in ids)
        {
            set.Add((id, singleId));
            set.Add((singleId, id));
        }
    }
}