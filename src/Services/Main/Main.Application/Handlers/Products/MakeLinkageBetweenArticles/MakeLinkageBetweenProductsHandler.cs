using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Persistence;
using Main.Application.Notifications;
using Main.Entities.Product;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.Products.MakeLinkageBetweenArticles;

[AutoSave]
[Transactional]
public record MakeLinkageBetweenProductsCommand(List<NewProductLinkageDto> Linkages) : ICommand<Unit>;

public class MakeLinkageBetweenProductsHandler(
    IPublisher publisher,
    IUnitOfWork unitOfWork,
    IProductRepository repository) : ICommandHandler<MakeLinkageBetweenProductsCommand, Unit>
{
    public async Task<Unit> Handle(MakeLinkageBetweenProductsCommand request, CancellationToken cancellationToken)
    {
        var linkages = request.Linkages;
        var updatedIds = new HashSet<int>();

        foreach (var linkage in linkages)
        {
            var ids = await CreateLinkages(linkage, cancellationToken);
            updatedIds.UnionWith(ids);
        }

        await publisher.Publish(new ArticlesUpdatedNotification(updatedIds), cancellationToken);
        return Unit.Value;
    }

    private async Task<IEnumerable<int>> CreateLinkages(
        NewProductLinkageDto linkage,
        CancellationToken cancellationToken = default)
    {
        List<int> productIds = [linkage.ProductId, linkage.CrossProductId];
        var criteria = Criteria<Product>.New()
            .Where(x => productIds.Contains(x.Id))
            .Track(false)
            .Build();
        
        var lrArticles = await repository.ListAsync(criteria, cancellationToken);

        var leftArticle = lrArticles.FirstOrDefault(x => x.Id == linkage.ProductId) 
                          ?? throw new ProductNotFoundException(linkage.ProductId);
        var rightArticle = lrArticles.FirstOrDefault(x => x.Id == linkage.CrossProductId) 
                           ?? throw new ProductNotFoundException(linkage.CrossProductId);

        var toAdd = new List<ProductCross>();

        switch (linkage.LinkageType)
        {
            case ProductLinkageType.SingleCross:
                toAdd.Add(ProductCross.Create(leftArticle.Id, rightArticle.Id));
                break;

            case ProductLinkageType.FullCross:
                var leftIds = await GetCrossIds(linkage.ProductId, cancellationToken);
                var rightIds = await GetCrossIds(linkage.CrossProductId, cancellationToken);

                leftIds.Add(leftArticle.Id);
                rightIds.Add(rightArticle.Id);
                
                var been = new HashSet<(int, int)>();
                
                foreach (var l in leftIds)
                foreach (var r in rightIds)
                {
                    var cross = ProductCross.Create(l, r);
                    if (been.Add(cross.GetId())) toAdd.Add(cross);
                }
                break;

            case ProductLinkageType.FullLeftToRightCross:
                var leftCrossIds = await GetCrossIds(linkage.ProductId, cancellationToken);
                leftCrossIds.Add(leftArticle.Id);
                foreach (var l in leftCrossIds)
                    toAdd.Add(ProductCross.Create(l, rightArticle.Id));
                break;

            case ProductLinkageType.FullRightToLeftCross:
                var rightCrossIds = await GetCrossIds(linkage.CrossProductId, cancellationToken);
                rightCrossIds.Add(rightArticle.Id);
                foreach (var r in rightCrossIds)
                    toAdd.Add(ProductCross.Create(leftArticle.Id, r));
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
        var ids = new HashSet<int>();
        foreach (var cross in toAdd)
        {
            ids.Add(cross.RightProductId);
            ids.Add(cross.LeftProductId);
        }
        
        return ids;
    }

    private async Task<HashSet<int>> GetCrossIds(int productId, CancellationToken ct)
    {
        var criteria = Criteria<Product>.New()
            .Track(false)
            .Build();
        return (await repository.GetProductCrosses(productId, criteria, ct))
            .Select(x => x.Id)
            .ToHashSet();
    }
}