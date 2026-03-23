using Analytics.Abstractions.Interfaces.DbRepositories;
using Application.Common.Interfaces;
using MediatR;

namespace Analytics.Application.Handlers.PurchaseFacts.CreatePurchaseFact;

public record CreatePurchaseFactCommand : ICommand;

public class CreatePurchaseFactHandler(IPurchaseFactRepository factRepository) : ICommandHandler<CreatePurchaseFactCommand>
{
    public async Task<Unit> Handle(CreatePurchaseFactCommand request, CancellationToken cancellationToken)
    {
        
    }
}