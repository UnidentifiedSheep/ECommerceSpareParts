using Main.Application.Extensions;
using Core.Attributes;
using Core.Dtos.Amw.ArticleReservations;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticleReservations;
using Main.Application.Interfaces;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.EditArticleReservation;

[Transactional]
public record EditArticleReservationCommand(int ReservationId, EditArticleReservationDto NewValue, Guid WhoUpdated)
    : ICommand;

public class EditArticleReservationHandler(
    IArticleReservationRepository reservationRepository,
    IUserRepository usersRepository,
    ICurrencyRepository currencyRepository,
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<EditArticleReservationCommand>
{
    public async Task<Unit> Handle(EditArticleReservationCommand request, CancellationToken cancellationToken)
    {
        await EnsureNeededExists(request.NewValue.ArticleId, request.NewValue.GivenCurrencyId, request.WhoUpdated,
            cancellationToken);
        var reservation =
            await reservationRepository.GetReservationAsync(request.ReservationId, true, cancellationToken)
            ?? throw new ReservationNotFoundException(request.ReservationId);
        request.NewValue.Adapt(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task EnsureNeededExists(int articleId, int? currencyId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        await articlesRepository.EnsureArticlesExist([articleId], cancellationToken);
        if (currencyId != null)
            await currencyRepository.EnsureCurrenciesExists([currencyId.Value], cancellationToken);
        await usersRepository.EnsureUsersExists([userId], cancellationToken);
    }
}