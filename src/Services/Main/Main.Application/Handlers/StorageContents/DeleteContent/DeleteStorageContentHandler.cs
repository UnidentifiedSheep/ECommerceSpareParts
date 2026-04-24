using System.Data;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Articles;
using Domain.Extensions;
using Exceptions;
using Exceptions.Base;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Application.Notifications;
using Main.Entities;
using Main.Entities.Event;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Storage;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.DeleteContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record DeleteStorageContentCommand(int ContentId, uint RowVersion) : ICommand;

public class DeleteStorageContentHandler(
    IRepository<StorageContent, int> repository,
    IUnitOfWork unitOfWork,
    IProductRepository productRepository,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<DeleteStorageContentCommand>
{
    public async Task<Unit> Handle(DeleteStorageContentCommand request, CancellationToken cancellationToken)
    {
        var id = request.ContentId;
        var content = await repository.GetById(request.ContentId, cancellationToken)
                      ?? throw new StorageContentNotFoundException(id);

        content.ValidateVersion(request.RowVersion);
        
        var product = await productRepository.EnsureProductExistsForUpdateAsync(content.ProductId, cancellationToken);

        await unitOfWork.AddAsync(
            StorageMovementEvent.Create(content, StorageMovementType.StorageContentDeletion), 
            cancellationToken);

        product.IncreaseStock(-content.Count);
        
        unitOfWork.Remove(content);
        
        integrationEventScope.Add(new ProductUpdatedEvent
        {
            Id = content.ProductId
        });

        return Unit.Value;
    }
}