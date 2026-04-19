using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.StorageOwners.AddStorageToUser;

[AutoSave]
[Transactional]
public record AddStorageToUserCommand(Guid UserId, string StorageName) : ICommand;

public class AddStorageToUserHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddStorageToUserCommand>
{
    public async Task<Unit> Handle(AddStorageToUserCommand request, CancellationToken cancellationToken)
    {
        StorageOwner model = StorageOwner.Create(request.StorageName, request.UserId);

        await unitOfWork.AddAsync(model, cancellationToken);
        return Unit.Value;
    }
}