using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.CreateArticleReservation;

[Transactional]
public record CreateArticleReservationCommand(List<NewArticleReservationDto> Reservations, Guid WhoCreated) : ICommand;

public class CreateArticleReservationHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateArticleReservationCommand>
{
    public async Task<Unit> Handle(CreateArticleReservationCommand request, CancellationToken cancellationToken)
    {
        var adaptedReservations = request.Reservations.Adapt<List<StorageContentReservation>>();
        
        foreach (var item in adaptedReservations)
            item.WhoCreated = request.WhoCreated;
        
        await unitOfWork.AddRangeAsync(adaptedReservations, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}