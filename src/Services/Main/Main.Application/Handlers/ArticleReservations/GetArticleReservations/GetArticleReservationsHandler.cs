using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.ArticleReservations.GetArticleReservations;

public record GetArticleReservationsQuery(string? SearchTerm, int Page, int Limit, string? SortBy, double? Similarity,
    Guid? UserId, GeneralSearchStrategy Strategy) : IQuery<GetArticleReservationsResult>;

public record GetArticleReservationsResult(IEnumerable<ArticleReservationDto> Reservations);

public class GetArticleReservationsHandler(IArticleReservationRepository reservationRepository)
    : IQueryHandler<GetArticleReservationsQuery, GetArticleReservationsResult>
{
    public async Task<GetArticleReservationsResult> Handle(GetArticleReservationsQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<StorageContentReservation> reservations;
        var offset = request.Page * request.Limit;
        var similarity = (request.Similarity ?? 0.5) >= 1 ? 0.999 : request.Similarity ?? 0.5;
        switch (request.Strategy)
        {
            case GeneralSearchStrategy.Exec:
                reservations = await reservationRepository.GetReservationsByExecAsync(request.SearchTerm,
                    request.UserId, offset, request.Limit, request.SortBy, false, cancellationToken);
                break;
            case GeneralSearchStrategy.Similarity:
                reservations = await reservationRepository.GetReservationsBySimilarityAsync(request.SearchTerm,
                    request.UserId, offset, request.Limit, request.SortBy, false, similarity, cancellationToken);
                break;
            case GeneralSearchStrategy.FromStart:
                reservations = await reservationRepository.GetReservationsFromStartAsync(request.SearchTerm,
                    request.UserId, offset, request.Limit, request.SortBy, false, cancellationToken);
                break;
            case GeneralSearchStrategy.Contains:
                reservations = await reservationRepository.GetReservationsContainsAsync(request.SearchTerm,
                    request.UserId, offset, request.Limit, request.SortBy, false, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var result = reservations.Adapt<List<ArticleReservationDto>>();
        return new GetArticleReservationsResult(result);
    }
}