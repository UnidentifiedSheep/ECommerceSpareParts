using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Storages;
using Exceptions.Exceptions.Users;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Users.AddStorageToUser;

[Transactional]
public record AddStorageToUserCommand(Guid UserId, string StorageName) : ICommand;

public class AddStorageToUserHandler(IUserRepository userRepository, IStoragesRepository storagesRepository, 
    IUnitOfWork unitOfWork) : ICommandHandler<AddStorageToUserCommand>
{
    public async Task<Unit> Handle(AddStorageToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByIdAsync(request.UserId, true, cancellationToken) ??
                   throw new UserNotFoundException(request.UserId);
        var storage = await storagesRepository.GetStorageAsync(request.StorageName, true, cancellationToken) ??
                      throw new StorageNotFoundException(request.StorageName);

        if (await userRepository.UserOwnsStorage(user.Id, storage.Name, cancellationToken))
            throw new UserAlreadyOwnsStorageException(user.Id, storage.Name);
        
        user.StorageNames.Add(storage);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}