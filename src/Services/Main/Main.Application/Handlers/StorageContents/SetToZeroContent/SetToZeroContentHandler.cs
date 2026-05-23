using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Products;
using Domain.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.StorageContents.SetToZeroContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record SetToZeroContentCommand(int ContentId, uint RowVersion) : ICommand;

public class SetToZeroContentHandler(
    IRepository<StorageContent, int> repository,
    IUnitOfWork unitOfWork,
    IProductRepository productRepository,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<SetToZeroContentCommand>
{
    public async Task<Unit> Handle(SetToZeroContentCommand request, CancellationToken cancellationToken)
    {
        var content = await repository.GetById(request.ContentId, cancellationToken)
                      ?? throw new StorageContentNotFoundException(request.ContentId);

        content.ValidateVersion(request.RowVersion);

        var product = await productRepository.EnsureExistForUpdateAsync(
            content.ProductId,
            id => new ProductNotFoundException(id),
            ct: cancellationToken);

        await unitOfWork.AddAsync(
            StorageMovementEvent.Create(content, StorageMovementType.StorageContentDeletion),
            cancellationToken);

        product.IncreaseStock(-content.Count);
        content.IncreaseCount(-content.Count);

        integrationEventScope.Add(new ProductUpdatedEvent
        {
            Id = content.ProductId
        });

        return Unit.Value;
    }
}