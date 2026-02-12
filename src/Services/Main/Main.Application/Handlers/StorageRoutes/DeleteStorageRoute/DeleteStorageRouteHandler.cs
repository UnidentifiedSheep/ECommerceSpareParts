using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.StorageRoutes;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.StorageRoutes.DeleteStorageRoute;

[Transactional]
public record DeleteStorageRouteCommand(Guid Id) : ICommand;

public class DeleteStorageRouteHandler(IStorageRoutesRepository storageRoutesRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<DeleteStorageRouteCommand>
{
    public async Task<Unit> Handle(DeleteStorageRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await storageRoutesRepository.GetStorageRouteAsync(request.Id, true, cancellationToken)
                    ?? throw new StorageRouteNotFound(request.Id);
        unitOfWork.Remove(route);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}