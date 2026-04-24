using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions.Storages;
using MediatR;

namespace Main.Application.Handlers.StorageRoutes.DeleteStorageRoute;

[AutoSave]
[Transactional]
public record DeleteStorageRouteCommand(Guid Id) : ICommand;

public class DeleteStorageRouteHandler(
    IStorageRouteRepository repository, 
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteStorageRouteCommand>
{
    public async Task<Unit> Handle(DeleteStorageRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await repository.GetById(request.Id, cancellationToken)
                    ?? throw new StorageRouteNotFound(request.Id);
        unitOfWork.Remove(route);
        return Unit.Value;
    }
}