using Application.Extensions;
using Application.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.ArticleReservations;
using Core.Exceptions.ArticleReservations;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Application.Handlers.ArticleReservations.EditArticleReservation;

[Transactional]
public record EditArticleReservationCommand(int ReservationId, EditArticleReservationDto NewValue, string WhoUpdated) : ICommand;

public class EditArticleReservationHandler(IArticleReservationRepository reservationRepository, IUsersRepository usersRepository, 
    ICurrencyRepository currencyRepository, IArticlesRepository articlesRepository, IUnitOfWork unitOfWork) : ICommandHandler<EditArticleReservationCommand>
{
    public async Task<Unit> Handle(EditArticleReservationCommand request, CancellationToken cancellationToken)
    {
        await EnsureNeededExists(request.NewValue.ArticleId, request.NewValue.GivenCurrencyId, request.WhoUpdated, cancellationToken);
        var reservation = await reservationRepository.GetReservationAsync(request.ReservationId, true, cancellationToken)
                          ?? throw new ReservationNotFoundException(request.ReservationId);
        request.NewValue.Adapt(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task EnsureNeededExists(int articleId, int? currencyId, string userId, CancellationToken cancellationToken = default)
    {
        await articlesRepository.EnsureArticlesExist([articleId], cancellationToken);
        if (currencyId != null)
            await currencyRepository.EnsureCurrenciesExists([currencyId.Value], cancellationToken);
        await usersRepository.EnsureUsersExists([userId], cancellationToken);
    }
}