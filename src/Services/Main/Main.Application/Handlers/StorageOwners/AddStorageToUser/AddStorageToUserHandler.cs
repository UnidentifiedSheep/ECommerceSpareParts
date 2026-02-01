using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Entities;
using MediatR;

namespace Main.Application.Handlers.StorageOwners.AddStorageToUser;

[Transactional]
public record AddStorageToUserCommand(Guid UserId, string StorageName) : ICommand;

public class AddStorageToUserHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddStorageToUserCommand>
{
    public async Task<Unit> Handle(AddStorageToUserCommand request, CancellationToken cancellationToken)
    {
        StorageOwner model = new()
        {
            OwnerId = request.UserId,
            StorageName = request.StorageName
        };
        
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}