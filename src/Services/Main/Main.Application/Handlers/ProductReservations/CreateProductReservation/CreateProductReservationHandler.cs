using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Entities.Storage;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.CreateArticleReservation;

[AutoSave]
[Transactional]
public record CreateProductReservationCommand(List<NewProductReservationDto> Reservations, Guid WhoCreated) : ICommand;

public class CreateProductReservationHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateProductReservationCommand>
{
    public async Task<Unit> Handle(CreateProductReservationCommand request, CancellationToken cancellationToken)
    {
        var reservations = new List<StorageContentReservation>();

        foreach (var dto in request.Reservations)
        {
            var newReservation = StorageContentReservation.Create(dto.UserId, dto.ProductId, dto.ReservedCount);
            
            newReservation.AddCount(dto.CurrentCount);
            newReservation.SetComment(dto.Comment);
            newReservation.ProposePrice(dto.ProposedPrice, dto.GivenCurrencyId);
            
            reservations.Add(newReservation);
        }

        await unitOfWork.AddRangeAsync(reservations, cancellationToken);

        return Unit.Value;
    }
}