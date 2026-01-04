using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Storages;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Storages.CreateStorage;

[Transactional]
public record CreateStorageCommand(string Name, string? Description, string? Location, StorageType Type) : ICommand<CreateStorageResult>;

public record CreateStorageResult(string Name);

public class CreateStorageHandler(
    IStoragesRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateStorageCommand, CreateStorageResult>
{
    public async Task<CreateStorageResult> Handle(CreateStorageCommand request, CancellationToken cancellationToken)
    {
        await ValidateData(request.Name, cancellationToken);
        var newStorage = request.Adapt<Storage>();
        await unitOfWork.AddAsync(newStorage, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateStorageResult(newStorage.Name);
    }

    private async Task ValidateData(string name, CancellationToken cancellationToken = default)
    {
        if (await repository.StorageExistsAsync(name, cancellationToken))
            throw new StorageNameIsTakenException(name);
    }
}