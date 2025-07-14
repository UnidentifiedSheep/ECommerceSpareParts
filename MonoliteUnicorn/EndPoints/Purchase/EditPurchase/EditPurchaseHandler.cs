using Core.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Purchase;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Purchase;

namespace MonoliteUnicorn.EndPoints.Purchase.EditPurchase;

public record EditPurchaseCommand(IEnumerable<EditPurchaseDto> Content, string PurchaseId, int CurrencyId, string? Comment, 
    DateTime PurchaseDateTime, string UpdatedUserId) : ICommand<Unit>;

public class EditPurchaseHandler(DContext context, IPurchaseOrchestrator orchestrator, CacheQueue cacheQueue) : ICommandHandler<EditPurchaseCommand, Unit>
{
    public async Task<Unit> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
    {
        var articleIds = await context.PurchaseContents
            .AsNoTracking()
            .Where(x => x.PurchaseId == request.PurchaseId)
            .Select(x => x.ArticleId)
            .ToHashSetAsync(cancellationToken);
        await orchestrator.EditPurchase(request.Content, request.PurchaseId, request.CurrencyId, request.Comment, request.UpdatedUserId, request.PurchaseDateTime, cancellationToken);
        articleIds.UnionWith(request.Content.Select(x => x.ArticleId));
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync(articleIds);
        });
        
        return Unit.Value;
    }
}