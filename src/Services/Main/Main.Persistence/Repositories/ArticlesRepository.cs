using System.Text;
using Abstractions.Models.Repository;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticlesRepository(DContext context) : IArticlesRepository
{
    public async Task<IReadOnlyList<Product>> GetArticleCrosses(
        QueryOptions<Product, int> options,
        CancellationToken cancellationToken = default)
    {
        return await context.Products
            .FromSql($"""
                      SELECT Distinct on (a.id) a.* 
                      FROM articles a 
                      JOIN article_crosses c ON a.id = c.article_id OR a.id = c.article_cross_id 
                                             WHERE c.article_id = {options.Data} OR 
                                                 c.article_cross_id = {options.Data}
                      ORDER BY a.id
                      """)
            .ApplyOptions(options)
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetArticleById(
        QueryOptions<Product, int> options, 
        CancellationToken cancellationToken = default)
    {
        return await context.Products
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.Id == options.Data, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetArticlesByIds(
        QueryOptions<Product, IReadOnlyList<int>> options,
        CancellationToken token = default)
    {
        return await context.Products
            .ApplyOptions(options)
            .Where(x => options.Data.Contains(x.Id))
            .ApplyPaging(options)
            .ToListAsync(token);
    }

    public async Task AddArticleLinkage(
        IEnumerable<(int id, int crossId)> crossIds,
        CancellationToken cancellationToken = default)
    {
        var hs = crossIds.ToHashSet();
        var queryBuilder = new StringBuilder("INSERT INTO article_crosses (article_id, article_cross_id) VALUES ");
        foreach (var (l, r) in hs)
            queryBuilder.Append($" ({l}, {r}),");
        queryBuilder.Length--;
        queryBuilder.Append(" ON CONFLICT DO NOTHING");
        await context.Database.ExecuteSqlRawAsync(queryBuilder.ToString(), cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetArticleCrossIds(int articleId, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .FromSql($"""
                      SELECT DISTINCT a.id
                      FROM articles a
                      JOIN article_crosses c
                        ON a.id = c.article_id
                        OR a.id = c.article_cross_id
                      WHERE c.article_id = {articleId}
                         OR c.article_cross_id = {articleId}
                      """)
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> UpdateArticlesCount(
        Dictionary<int, int> toIncrement,
        CancellationToken cancellationToken = default)
    {
        var valuesSql = string.Join(", ", toIncrement.Select(i => $"({i.Key}, {i.Value})"));

        var query = $"""
                     UPDATE articles a
                     SET total_count = a.total_count + data.increment
                     FROM (VALUES {valuesSql}) AS data(article_id, increment)
                     WHERE a.id = data.article_id
                     """;
        return await context.Database.ExecuteSqlRawAsync(query, cancellationToken);
    }
}