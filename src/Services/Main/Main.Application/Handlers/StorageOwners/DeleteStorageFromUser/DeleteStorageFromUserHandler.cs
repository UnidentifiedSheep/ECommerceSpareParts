using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.StorageOwner;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.StorageOwners.DeleteStorageFromUser;

[Transactional]
public record DeleteStorageFromUserCommand(Guid UserId, string StorageName) : ICommand;

public class DeleteStorageFromUserHandler(IStorageOwnersRepository storageOwnersRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<DeleteStorageFromUserCommand>
{
    public async Task<Unit> Handle(DeleteStorageFromUserCommand request, CancellationToken cancellationToken)
    {
        var model = await storageOwnersRepository.GetStorageOwnerAsync(request.UserId, request.StorageName,
            true, cancellationToken) ?? throw new StorageOwnerNotFoundException(request.UserId, request.StorageName);
        unitOfWork.Remove(model);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}