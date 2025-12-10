using System.Linq.Expressions;
using Core.Extensions;
using Main.Core.Entities;
using Main.Core.Extensions;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleReservationRepository(DContext context) : IArticleReservationRepository
{
    public async Task<bool> ReservationExists(int reservationId, CancellationToken cancellationToken = default)
    {
        return await context.StorageContentReservations
            .AsNoTracking()
            .AnyAsync(x => x.Id == reservationId, cancellationToken);
    }

    public async Task<StorageContentReservation?> GetReservationAsync(int reservationId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageContentReservations
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
    }

    public async Task<IEnumerable<StorageContentReservation>> GetReservationsByExecAsync(string? searchTerm,
        Guid? userId,
        int offset, int limit, string? sortBy, bool track = true, CancellationToken cancellationToken = default)
    {
        searchTerm = searchTerm?.Trim();
        var query = GetReservationsInternal(userId, track, sortBy);
        query = query.Where(x => string.IsNullOrWhiteSpace(searchTerm) || x.Comment == searchTerm ||
                                 x.Article.NormalizedArticleNumber == searchTerm.ToNormalizedArticleNumber() ||
                                 x.Comment == searchTerm || x.Article.ArticleName == searchTerm);
        return await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StorageContentReservation>> GetReservationsBySimilarityAsync(string? searchTerm,
        Guid? userId,
        int offset, int limit, string? sortBy, bool track = true, double similarity = 0.5,
        CancellationToken cancellationToken = default)
    {
        var query = GetReservationsInternal(userId, track, sortBy);
        var queryWithRank = query.Where(x => string.IsNullOrWhiteSpace(searchTerm) ||
                                             x.Comment == null || EF.Functions.TrigramsSimilarity(x.Comment.ToUpper(),
                                                 searchTerm.ToUpper()) >= similarity ||
                                             EF.Functions.TrigramsSimilarity(x.Article.NormalizedArticleNumber,
                                                 searchTerm.ToNormalizedArticleNumber()) >= similarity ||
                                             EF.Functions.ToTsVector("russian", x.Article.ArticleName)
                                                 .Matches(EF.Functions.PlainToTsQuery("russian", searchTerm)))
            .Select(x => new
            {
                Reservation = x,
                Rank = string.IsNullOrWhiteSpace(searchTerm)
                    ? 0
                    : EF.Functions.Greatest(EF.Functions.ToTsVector("russian", x.Article.ArticleName)
                            .Rank(EF.Functions.PlainToTsQuery("russian", searchTerm)),
                        EF.Functions.TrigramsSimilarity(x.Article.NormalizedArticleNumber,
                            searchTerm.ToNormalizedArticleNumber()),
                        x.Comment == null
                            ? 0
                            : EF.Functions.TrigramsSimilarity(x.Comment.ToUpper(), searchTerm.ToUpper()))
            });

        if (string.IsNullOrWhiteSpace(sortBy) || sortBy.Contains("relevance"))
            queryWithRank = sortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);

        query = queryWithRank
            .Select(x => x.Reservation);

        if (!string.IsNullOrWhiteSpace(sortBy) && !sortBy.Contains("relevance"))
            query = query.SortBy(sortBy);

        return await query.Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StorageContentReservation>> GetReservationsFromStartAsync(string? searchTerm,
        Guid? userId,
        int offset, int limit, string? sortBy, bool track = true, CancellationToken cancellationToken = default)
    {
        searchTerm = searchTerm?.Trim();
        var query = GetReservationsInternal(userId, track, sortBy);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(x =>
                (x.Comment != null && EF.Functions.ILike(x.Comment, $"{searchTerm}%")) ||
                EF.Functions.ILike(x.Article.NormalizedArticleNumber, $"{searchTerm.ToNormalizedArticleNumber()}%") ||
                EF.Functions.ILike(x.Article.ArticleName, $"{searchTerm}%")
            );

        if (!string.IsNullOrWhiteSpace(sortBy))
            query = query.SortBy(sortBy);

        return await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StorageContentReservation>> GetReservationsContainsAsync(string? searchTerm,
        Guid? userId,
        int offset, int limit, string? sortBy, bool track = true, CancellationToken cancellationToken = default)
    {
        searchTerm = searchTerm?.Trim();
        var query = GetReservationsInternal(userId, track, sortBy);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(x =>
                (x.Comment != null && EF.Functions.ILike(x.Comment, $"%{searchTerm}%")) ||
                EF.Functions.ILike(x.Article.NormalizedArticleNumber, $"%{searchTerm.ToNormalizedArticleNumber()}%") ||
                EF.Functions.ILike(x.Article.ArticleName, $"%{searchTerm}%")
            );

        if (!string.IsNullOrWhiteSpace(sortBy))
            query = query.SortBy(sortBy);

        return await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task<Dictionary<int, int>> GetReservationsCountForUserAsync(Guid userId, IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default)
    {
        return GetReservationsCountInternalAsync(
            r => r.UserId == userId && articleIds.Contains(r.ArticleId) && !r.IsDone,
            cancellationToken);
    }

    public Task<Dictionary<int, int>> GetReservationsCountForOthersAsync(
        Guid userId, IEnumerable<int> articleIds, CancellationToken cancellationToken = default)
    {
        return GetReservationsCountInternalAsync(
            r => r.UserId != userId && articleIds.Contains(r.ArticleId) && !r.IsDone,
            cancellationToken);
    }

    public async Task<Dictionary<int, List<StorageContentReservation>>> GetUserReservations(Guid userId,
        IEnumerable<int> articleIds, bool isDone = false, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageContentReservations
            .ConfigureTracking(track)
            .Where(x => x.UserId == userId &&
                        articleIds.Contains(x.ArticleId) &&
                        x.IsDone == isDone)
            .GroupBy(x => x.ArticleId)
            .ToDictionaryAsync(g => g.Key,
                g => g.ToList(), cancellationToken);
    }

    public async Task<Dictionary<int, List<StorageContentReservation>>> GetUserReservationsForUpdate(Guid userId,
        IEnumerable<int> articleIds, bool isDone = false, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var list = await context.StorageContentReservations
            .FromSql($"""
                      SELECT *
                      FROM storage_content_reservations
                      WHERE user_id = {userId}
                        AND is_done = {isDone}
                        AND article_id = ANY({articleIds})
                      FOR UPDATE
                      """)
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);

        return list
            .GroupBy(x => x.ArticleId)
            .ToDictionary(
                g => g.Key,
                g => g.ToList());
    }

    private IQueryable<StorageContentReservation> GetReservationsInternal(Guid? userId, bool track, string? sortBy)
    {
        var query = context.StorageContentReservations
            .SortBy(sortBy)
            .ConfigureTracking(track);
        if (userId != null)
            query = query.Where(x => x.UserId == userId);
        return query;
    }

    private Task<Dictionary<int, int>> GetReservationsCountInternalAsync(
        Expression<Func<StorageContentReservation, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return context.StorageContentReservations
            .AsNoTracking()
            .Where(predicate)
            .GroupBy(x => x.ArticleId)
            .Select(g => new
            {
                ArticleId = g.Key,
                TotalCount = g.Sum(y => y.CurrentCount)
            })
            .ToDictionaryAsync(
                x => x.ArticleId,
                x => x.TotalCount,
                cancellationToken);
    }
}