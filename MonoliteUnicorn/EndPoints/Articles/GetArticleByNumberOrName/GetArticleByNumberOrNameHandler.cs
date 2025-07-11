﻿using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.SearchLog;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleByNumberOrName;

public record GetArticleByNumberOrNameQuery(string SearchTerm, int Page, int ViewCount, string? SortBy, IEnumerable<int> ProducerIds, string? UserId) : IQuery<GetArticleByNumberOrNameResult>;
public record GetArticleByNumberOrNameResult(IEnumerable<ArticleDto> Articles);

public class GetArticleByNumberOrNameValidation : AbstractValidator<GetArticleByNumberOrNameQuery>
{
    public GetArticleByNumberOrNameValidation()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage("Минимальная длинна строки поиска 3");
        
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}
public class GetArticleByNumberOrNameHandler(DContext context, ISearchLogger searchLogger) : IQueryHandler<GetArticleByNumberOrNameQuery, GetArticleByNumberOrNameResult>
{
    public async Task<GetArticleByNumberOrNameResult> Handle(GetArticleByNumberOrNameQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var searchModel = new SearchLogModel(request.UserId, "Articles_FullSearch", request);
            searchLogger.Enqueue(searchModel);
        }
        
        var normalizedSearchTerm = request.SearchTerm.ToNormalizedArticleNumber();

        var queryWithRank = context.Articles
            .AsNoTracking()
            .Where(x =>
                EF.Functions.ToTsVector("russian", x.ArticleName)
                    .Matches(EF.Functions.PlainToTsQuery("russian", request.SearchTerm)) ||
                EF.Functions.ILike(x.NormalizedArticleNumber, $"%{normalizedSearchTerm}%"))
            .Select(x => new
            {
                Article = x,
                Rank = Math.Max(
                    EF.Functions.ToTsVector("russian", x.ArticleName)
                        .Rank(EF.Functions.PlainToTsQuery("russian", request.SearchTerm)),
                    EF.Functions.ToTsVector("russian", x.NormalizedArticleNumber)
                        .Rank(EF.Functions.PlainToTsQuery("russian", normalizedSearchTerm))
                )
            });
        
        if (request.ProducerIds.Any())
            queryWithRank = queryWithRank.Where(x => request.ProducerIds.Contains(x.Article.ProducerId));
        
        
        if (string.IsNullOrWhiteSpace(request.SortBy) || request.SortBy.Contains("relevance"))
        {
            queryWithRank = request.SortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);
        }
        
        var query = queryWithRank
            .Select(x => x.Article);
        
        if (!string.IsNullOrWhiteSpace(request.SortBy) && !request.SortBy.Contains("relevance"))
            query = query.SortBy(request.SortBy);
        
        query = query
            .Skip(request.ViewCount * request.Page)
            .Take(request.ViewCount);
        
        var articles = await query
            .AsSplitQuery()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken);
        
        return new GetArticleByNumberOrNameResult(articles.Adapt<List<ArticleDto>>());
    }
}