using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Storages;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
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
        var storage = await repository.GetStorageAsync(request.StorageName, cancellationToken: cancellationToken,
                          includes: x => x.Owners)
                      ?? throw new StorageNotFoundException(request.StorageName);
        var editType = request.EditStorage.Type;
        if (editType.IsSet &&  storage.Type != editType.Value && storage.Owners.Count > 0)
            throw new ChangeOfStorageTypeRestrictedException("Присутствуют владельцы склада");
        
        request.EditStorage.Adapt(storage);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}