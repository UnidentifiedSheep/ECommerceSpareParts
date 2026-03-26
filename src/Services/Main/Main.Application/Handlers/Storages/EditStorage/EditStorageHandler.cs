using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Exceptions.Storages;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Storages.EditStorage;

[Transactional]
public record EditStorageCommand(string StorageName, PatchStorageDto EditStorage) : ICommand;

public class EditStorageHandler(IStoragesRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<EditStorageCommand>
{
    public async Task<Unit> Handle(EditStorageCommand request, CancellationToken cancellationToken)
    {
        var storage = await repository.GetStorageAsync(request.StorageName, true, cancellationToken,
                          x => x.StorageOwners)
                      ?? throw new StorageNotFoundException(request.StorageName);
        var editType = request.EditStorage.Type;
        if (editType.IsSet && storage.Type != editType.Value && storage.StorageOwners.Count > 0)
            throw new ChangeOfStorageTypeRestrictedException();

        request.EditStorage.Adapt(storage);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}