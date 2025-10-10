using Application.Common.Interfaces;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Storages;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Storages.DeleteStorage;

public record DeleteStorageCommand(string StorageName) : ICommand;

public class DeleteStorageHandler(IStoragesRepository storagesRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteStorageCommand>
{
    public async Task<Unit> Handle(DeleteStorageCommand request, CancellationToken cancellationToken)
    {
        var storage = await storagesRepository.GetStorageAsync(request.StorageName, true, cancellationToken)
                      ?? throw new StorageNotFoundException(request.StorageName);
        unitOfWork.Remove(storage);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}