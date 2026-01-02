using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticleReservations;
using Exceptions.Exceptions.Articles;
using Exceptions.Exceptions.Currencies;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.ArticleReservations;
using Main.Core.Interfaces.DbRepositories;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.EditArticleReservation;

[Transactional]
public record EditArticleReservationCommand(int ReservationId, EditArticleReservationDto NewValue, Guid WhoUpdated)
    : ICommand;

public class EditArticleReservationHandler(
    IArticleReservationRepository reservationRepository,
    DbDataValidatorBase dbValidator,
    IUnitOfWork unitOfWork) : ICommandHandler<EditArticleReservationCommand>
{
    public async Task<Unit> Handle(EditArticleReservationCommand request, CancellationToken cancellationToken)
    {
        await EnsureNeededExists(request.NewValue.ArticleId, request.NewValue.GivenCurrencyId, cancellationToken);
        var reservation =
            await reservationRepository.GetReservationAsync(request.ReservationId, true, cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);
        request.NewValue.Adapt(reservation);
        reservation.WhoUpdated = request.WhoUpdated;
        reservation.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task EnsureNeededExists(int articleId, int? currencyId, CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan()
            .EnsureArticleExists(articleId);
        if (currencyId != null)
            plan.EnsureCurrencyExists(currencyId.Value);
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}