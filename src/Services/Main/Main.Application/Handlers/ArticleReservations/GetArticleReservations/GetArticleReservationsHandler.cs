using Abstractions.Models;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.ArticleReservations.GetArticleReservations;

public record GetArticleReservationsQuery(string? SearchTerm, PaginationModel Pagination, string? SortBy, double? Similarity,
    Guid? UserId, GeneralSearchStrategy Strategy) : IQuery<GetArticleReservationsResult>;

public record GetArticleReservationsResult(IEnumerable<ArticleReservationDto> Reservations);

public class GetArticleReservationsHandler(IArticleReservationRepository reservationRepository)
    : IQueryHandler<GetArticleReservationsQuery, GetArticleReservationsResult>
{
    public async Task<GetArticleReservationsResult> Handle(GetArticleReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;
        IEnumerable<StorageContentReservation> reservations;
        var offset = page * size;
        var similarity = (request.Similarity ?? 0.5) >= 1 ? 0.999 : request.Similarity ?? 0.5;
        reservations = request.Strategy switch
        {
            GeneralSearchStrategy.Exec => await reservationRepository.GetReservationsByExecAsync(request.SearchTerm,
                request.UserId, offset, size, request.SortBy, false, cancellationToken),
            GeneralSearchStrategy.Similarity => await reservationRepository.GetReservationsBySimilarityAsync(
                request.SearchTerm, request.UserId, offset, size, request.SortBy, false, similarity, cancellationToken),
            GeneralSearchStrategy.FromStart => await reservationRepository.GetReservationsFromStartAsync(
                request.SearchTerm, request.UserId, offset, size, request.SortBy, false, cancellationToken),
            GeneralSearchStrategy.Contains => await reservationRepository.GetReservationsContainsAsync(
                request.SearchTerm, request.UserId, offset, size, request.SortBy, false, cancellationToken),
            _ => throw new ArgumentOutOfRangeException()
        };

        var result = reservations.Adapt<List<ArticleReservationDto>>();
        return new GetArticleReservationsResult(result);
    }
}