using Core.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Purchase;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Purchase;

namespace MonoliteUnicorn.EndPoints.Purchase.DeletePurchase;

public record DeletePurchaseCommand(string PurchaseId, string UserId) : ICommand<Unit>;

public class DeletePurchaseValidation : AbstractValidator<DeletePurchaseCommand>
{
    public DeletePurchaseValidation()
    {
        RuleFor(x => x.PurchaseId).NotEmpty().WithMessage("Айди закупки не должен быть пустым");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Айди пользователя не может быть пуст");
    }
}

public class DeletePurchaseHandler(DContext context, IPurchaseOrchestrator orchestrator, CacheQueue cacheQueue) : ICommandHandler<DeletePurchaseCommand, Unit>
{
    public async Task<Unit> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        var articleIds = await context.PurchaseContents.AsNoTracking()
            .Where(x => x.PurchaseId == request.PurchaseId)
            .Select(x => x.ArticleId)
            .ToHashSetAsync(cancellationToken);
        await orchestrator.DeletePurchase(request.PurchaseId, request.UserId, cancellationToken);
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync(articleIds);
        });
        return Unit.Value;
    }
}