using Core.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Sale;

namespace MonoliteUnicorn.EndPoints.Sales.DeleteSale;

public record DeleteSaleCommand(string SaleId, string UserId) : ICommand;


public class DeleteSaleHandler(DContext context, ISaleOrchestrator orchestrator, CacheQueue cacheQueue) : ICommandHandler<DeleteSaleCommand>
{
    public async Task<Unit> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var articleIds = await context.SaleContents.AsNoTracking()
            .Where(x => x.SaleId == request.SaleId)
            .Select(x => x.ArticleId)
            .ToHashSetAsync(cancellationToken);
        await orchestrator.DeleteSale(request.SaleId, request.UserId, cancellationToken);
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync(articleIds);
        });
        return Unit.Value;
    }
}