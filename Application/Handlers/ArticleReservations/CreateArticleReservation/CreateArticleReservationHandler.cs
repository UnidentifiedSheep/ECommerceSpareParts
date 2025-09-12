using Application.Extensions;
using Application.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.ArticleReservations;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Application.Handlers.ArticleReservations.CreateArticleReservation;

[Transactional]
public record CreateArticleReservationCommand(List<NewArticleReservationDto> Reservations, string WhoCreated)
    : ICommand;

public class CreateArticleReservationHandler(
    IArticlesRepository articlesRepository,
    IUsersRepository usersRepository,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateArticleReservationCommand>
{
    public async Task<Unit> Handle(CreateArticleReservationCommand request, CancellationToken cancellationToken)
    {
        var reservations = request.Reservations.ToList();
        var currencyIds = new HashSet<int>();
        var articleIds = new HashSet<int>();
        var userIds = new HashSet<string> { request.WhoCreated };

        foreach (var item in reservations)
        {
            if (item.GivenCurrencyId != null)
                currencyIds.Add(item.GivenCurrencyId.Value);
            articleIds.Add(item.ArticleId);
            userIds.Add(item.UserId);
        }

        await CheckIfNeededExists(currencyIds, articleIds, userIds, cancellationToken);

        var adaptedReservations = reservations.Adapt<List<StorageContentReservation>>();
        foreach (var item in adaptedReservations)
            item.WhoCreated = request.WhoCreated;
        await unitOfWork.AddRangeAsync(adaptedReservations, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task CheckIfNeededExists(IEnumerable<int> currencyIds, IEnumerable<int> articleIds,
        IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        await currencyRepository.EnsureCurrenciesExists(currencyIds, cancellationToken);
        await usersRepository.EnsureUsersExists(userIds, cancellationToken);
        await articlesRepository.EnsureArticlesExist(articleIds, cancellationToken);
    }
}