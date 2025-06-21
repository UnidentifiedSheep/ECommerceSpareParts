using Core.Interface;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Purchase;
using MonoliteUnicorn.Services.Purchase;

namespace MonoliteUnicorn.EndPoints.Purchase.EditPurchase;

public record EditPurchaseCommand(IEnumerable<EditPurchaseDto> Content, string PurchaseId, int CurrencyId, string? Comment, 
    DateTime PurchaseDateTime, string UpdatedUserId) : ICommand<Unit>;

public class EditPurchaseHandler(IPurchaseOrchestrator orchestrator) : ICommandHandler<EditPurchaseCommand, Unit>
{
    public async Task<Unit> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
    {
        await orchestrator.EditPurchase(request.Content, request.PurchaseId, request.CurrencyId, request.Comment, request.UpdatedUserId, request.PurchaseDateTime, cancellationToken);
        return Unit.Value;
    }
}