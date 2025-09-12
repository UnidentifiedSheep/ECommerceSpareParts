using Application.Interfaces;
using Core.Dtos.Amw.Storage;
using Core.Exceptions.Storages;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Application.Handlers.Storages.EditStorage;

public record EditStorageCommand(string StorageName, PatchStorageDto EditStorage) : ICommand;
public class EditStorageHandler(IStoragesRepository repository, IUnitOfWork unitOfWork) : ICommandHandler<EditStorageCommand>
{
    public async Task<Unit> Handle(EditStorageCommand request, CancellationToken cancellationToken)
    {
        var storage = await repository.GetStorageAsync(request.StorageName, true, cancellationToken)
                      ?? throw new StorageNotFoundException(request.StorageName);
        request.EditStorage.Adapt(storage);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}