using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Contracts.Purchase;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Entities;
using Main.Entities.Balance;
using Main.Entities.Purchase;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;
using ContractPurchase = Contracts.Models.Purchase.Purchase;

namespace Main.Application.Handlers.Purchases.CreateFullPurchase;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record CreateFullPurchaseCommand(
    Guid CreatedUserId,
    Guid SupplierId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum,
    bool WithLogistics,
    string? StorageFrom) : ICommand;

public class CreateFullPurchaseHandler(
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork,
    IPurchaseService purchaseService) : ICommandHandler<CreateFullPurchaseCommand>
{
    public async Task<Unit> Handle(CreateFullPurchaseCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}