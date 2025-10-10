using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Application.Extensions;
using Main.Core.Interfaces.DbRepositories;

namespace Main.Application.Handlers.ArticleReservations.SubtractCountFromReservations;

[Transactional]
public record SubtractCountFromReservationsCommand(Guid UserId, Guid WhoUpdated, Dictionary<int, int> Contents)
    : ICommand<SubtractCountFromReservationsResult>;

/// <param name="NotFoundReservations">Id артикулов и количество, которые не были найдены в резервациях у пользователя.</param>
public record SubtractCountFromReservationsResult(Dictionary<int, int> NotFoundReservations);

public class SubtractCountFromReservationsHandler(
    IArticleReservationRepository reservationRepository,
    IUserRepository usersRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SubtractCountFromReservationsCommand, SubtractCountFromReservationsResult>
{
    public async Task<SubtractCountFromReservationsResult> Handle(SubtractCountFromReservationsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Contents.Count == 0)
            return new SubtractCountFromReservationsResult([]);
        var userId = request.UserId;
        var whoUpdated = request.WhoUpdated;
        var remaining = new Dictionary<int, int>(request.Contents);
        await EnsureDataExists([userId, whoUpdated], cancellationToken);

        var articleIds = remaining.Keys;
        var reservationsByIds = await reservationRepository.GetUserReservationsForUpdate(userId, articleIds,
            false, true, cancellationToken);
        foreach (var articleId in articleIds)
        {
            if (!reservationsByIds.TryGetValue(articleId, out var reservations))
                continue;

            foreach (var reservation in reservations)
            {
                var subtractCount = remaining[articleId];
                if (subtractCount <= 0)
                    break;

                var min = Math.Min(subtractCount, reservation.CurrentCount);
                reservation.CurrentCount -= min;
                remaining[articleId] -= min;

                if (reservation.CurrentCount == 0)
                    reservation.IsDone = true;
                reservation.WhoUpdated = whoUpdated;
                reservation.UpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var notFoundReservations = remaining.Where(x => x.Value > 0)
            .ToDictionary(x => x.Key, x => x.Value);
        return new SubtractCountFromReservationsResult(notFoundReservations);
    }

    private async Task EnsureDataExists(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        await usersRepository.EnsureUsersExists(userIds, cancellationToken);
    }
}