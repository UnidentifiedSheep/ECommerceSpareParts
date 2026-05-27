using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using MediatR;

namespace Main.Application.Handlers.Purchases.DeletePurchase;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record DeletePurchaseCommand(Guid PurchaseId) : ICommand<Unit>;

public class DeletePurchaseHandler(
    IUnitOfWork unitOfWork,
    IRepository<Purchase, Guid> repository,
    IRepository<PurchaseContent, int> purchaseContentRepository,
    ISender sender)
    : ICommandHandler<DeletePurchaseCommand, Unit>
{
    public async Task<Unit> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchase = await GetPurchase(request.PurchaseId, cancellationToken);
        throw new NotImplementedException();
    }

    private async Task<Purchase> GetPurchase(Guid id, CancellationToken cancellationToken)
    {
        var purchase = await repository.EnsureExistForUpdateAsync(
            id,
            key => new PurchaseNotFoundException(key),
            null,
            cancellationToken);

        var criteria = Criteria<PurchaseContent>
            .New()
            .Include(x => x.Product)
            .Include(x => x.StorageContentId)
            .Track()
            .Build();
        await purchaseContentRepository.ListAsync(criteria, cancellationToken);
        return purchase;
    }
}