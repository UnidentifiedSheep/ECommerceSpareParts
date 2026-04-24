using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.Storages.DeleteStorage;

[AutoSave]
[Transactional]
public record DeleteStorageCommand(string StorageName) : ICommand;

public class DeleteStorageHandler(
    IRepository<Storage, string> repository, 
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteStorageCommand>
{
    public async Task<Unit> Handle(DeleteStorageCommand request, CancellationToken cancellationToken)
    {
        var storage = await repository.GetById(request.StorageName, cancellationToken)
                      ?? throw new StorageNotFoundException(request.StorageName);
        
        unitOfWork.Remove(storage);
        return Unit.Value;
    }
}