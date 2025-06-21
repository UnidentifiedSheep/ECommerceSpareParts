using Core.Interface;
using FluentValidation;
using MediatR;
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

public class DeletePurchaseHandler(IPurchaseOrchestrator orchestrator) : ICommandHandler<DeletePurchaseCommand, Unit>
{
    public async Task<Unit> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        await orchestrator.DeletePurchase(request.PurchaseId, request.UserId, cancellationToken);
        return Unit.Value;
    }
}