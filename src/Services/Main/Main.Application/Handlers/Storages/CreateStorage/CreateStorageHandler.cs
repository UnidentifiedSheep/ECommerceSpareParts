using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.Storages.CreateStorage;

[Transactional]
public record CreateStorageCommand(string Name, string? Description, string? Location, StorageType Type) : ICommand<CreateStorageResult>;

public record CreateStorageResult(string Name);

public class CreateStorageHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateStorageCommand, CreateStorageResult>
{
    public async Task<CreateStorageResult> Handle(CreateStorageCommand request, CancellationToken cancellationToken)
    {
        var newStorage = request.Adapt<Storage>();
        await unitOfWork.AddAsync(newStorage, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateStorageResult(newStorage.Name);
    }
}