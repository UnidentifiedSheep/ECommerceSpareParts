using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Dtos.Amw.Purchase;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.Purchases.EditFullPurchase;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record EditFullPurchaseCommand(
    IEnumerable<EditPurchaseDto> Content,
    string PurchaseId,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime,
    Guid UpdatedUserId,
    bool WithLogistics,
    string? StorageFrom) : ICommand;

public class EditFullPurchaseHandler(
    IMediator mediator,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IPurchaseService purchaseService) : ICommandHandler<EditFullPurchaseCommand>
{
    public async Task<Unit> Handle(EditFullPurchaseCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}