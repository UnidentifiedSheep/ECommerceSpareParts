using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.StorageOwners.DeleteStorageFromUser;

[AutoSave]
[Transactional]
public record DeleteStorageFromUserCommand(Guid UserId, string StorageName) : ICommand;

public class DeleteStorageFromUserHandler(
    IRepository<StorageOwner, (string, Guid)> repository,
    IUnitOfWork unitOfWork
)
    : ICommandHandler<DeleteStorageFromUserCommand>
{
    public async Task<Unit> Handle(DeleteStorageFromUserCommand request, CancellationToken cancellationToken)
    {
        var model = await repository.GetById((request.StorageName, request.UserId), cancellationToken)
                    ?? throw new StorageOwnerNotFoundException(request.UserId, request.StorageName);
        unitOfWork.Remove(model);
        return Unit.Value;
    }
}