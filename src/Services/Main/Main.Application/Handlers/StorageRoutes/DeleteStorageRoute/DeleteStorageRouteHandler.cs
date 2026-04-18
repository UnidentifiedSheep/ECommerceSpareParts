using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Exceptions.Storages;
using Main.Application.Interfaces.Repositories;
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