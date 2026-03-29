using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;

namespace Main.Application.Handlers.ArticleReservations.SubtractCountFromReservations;

[Transactional]
public record SubtractCountFromReservationsCommand(Guid UserId, Guid WhoUpdated, Dictionary<int, int> Contents)
    : ICommand<SubtractCountFromReservationsResult>;

/// <param name="NotFoundReservations">Id артикулов и количество, которые не были найдены в резервациях у пользователя.</param>
public record SubtractCountFromReservationsResult(Dictionary<int, int> NotFoundReservations);

public class SubtractCountFromReservationsHandler(
    IArticleReservationRepository reservationRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SubtractCountFromReservationsCommand, SubtractCountFromReservationsResult>
{
    public async Task<SubtractCountFromReservationsResult> Handle(
        SubtractCountFromReservationsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Contents.Count == 0)
            return new SubtractCountFromReservationsResult([]);
        var userId = request.UserId;
        var whoUpdated = request.WhoUpdated;
        var remaining = new Dictionary<int, int>(request.Contents);

        var articleIds = remaining.Keys;
        
        var queryOptions = new QueryOptions<StorageContentReservation, GetUserReservationsOptionsData>()
        {
            Data = new GetUserReservationsOptionsData
            {
                ArticleIds = articleIds.ToList(),
                UserId = userId,
                IsDone = false
            }
        }.WithTracking();
        
        var reservationsByIds = await reservationRepository
            .GetUserReservations(queryOptions, cancellationToken);
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
                reservation.UpdatedAt = DateTime.Now;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var notFoundReservations = remaining.Where(x => x.Value > 0)
            .ToDictionary(x => x.Key, x => x.Value);
        return new SubtractCountFromReservationsResult(notFoundReservations);
    }
}