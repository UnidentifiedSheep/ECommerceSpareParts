using Core.Interface;
using MediatR;
using MonoliteUnicorn.Services.Sale;

namespace MonoliteUnicorn.EndPoints.Sales.DeleteSale;

public record DeleteSaleCommand(string SaleId, string UserId) : ICommand;


public class DeleteSaleHandler(ISaleOrchestrator orchestrator) : ICommandHandler<DeleteSaleCommand>
{
    public async Task<Unit> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        await orchestrator.DeleteSale(request.SaleId, request.UserId, cancellationToken);
        return Unit.Value;
    }
}