using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Purchase;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Purchases.EditPurchase;
using Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Main.Entities;
using Main.Entities.Exceptions.Purchase;
using Main.Entities.Purchase;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;
using ContractPurchase = Contracts.Models.Purchase.Purchase;

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