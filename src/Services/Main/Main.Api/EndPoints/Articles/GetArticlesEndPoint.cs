﻿using System.Security.Claims;
using Carter;
using Core.Models;
using Core.StaticFunctions;
using Main.Application.Handlers.Articles.GetArticles;
using Main.Core.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Security.Extensions;
using AmwArticleDto = Main.Core.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = Main.Core.Dtos.Anonymous.Articles.ArticleDto;

namespace Main.Api.EndPoints.Articles;

public record GetArticleViaStartNumberAmwResponse(IEnumerable<AmwArticleDto> Articles);

public record GetArticleViaStartNumberAnonymousResponse(IEnumerable<AnonymousArticleDto> Articles);

public record GetArticleViaStartNumberRequest(
    [FromQuery(Name = "searchTerm")] string SearchTerm,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "limit")] int Limit,
    [FromQuery(Name = "sortBy")] string? SortBy,
    [FromQuery(Name = "searchStrategy")] ArticleSearchStrategy SearchStrategy);

public class GetArticlesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles", async (ISender sender, HttpContext context, ClaimsPrincipal user,
                [AsParameters] GetArticleViaStartNumberRequest request, CancellationToken token) =>
            {
                var userId = user.GetUserId();
                var roles = user.GetUserRoles();
                var producerIds = context.Request.Query["producerId"]
                    .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();
                var pagination = new PaginationModel(request.Page, request.Limit);
                if (roles.IsAnyMatchInvariant("admin", "moderator", "worker"))
                    return await GetAmw(sender, request, pagination, userId, producerIds, token);
                return await GetAnonymous(sender, request, pagination, userId, producerIds, token);
            }).WithTags("Articles")
            .WithDescription("Поиск артикула с начала номера")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Поиск артикула с начала номера");
    }

    private async Task<IResult> GetAmw(ISender sender, GetArticleViaStartNumberRequest request,
        PaginationModel pagination,
        string? userId, IEnumerable<int> producerIds, CancellationToken token)
    {
        var query = new GetArticlesQuery<AmwArticleDto>(request.SearchTerm, pagination, request.SortBy, producerIds,
            request.SearchStrategy, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleViaStartNumberAmwResponse>();
        return Results.Ok(response);
    }

    private async Task<IResult> GetAnonymous(ISender sender, GetArticleViaStartNumberRequest request,
        PaginationModel pagination,
        string? userId, IEnumerable<int> producerIds, CancellationToken token)
    {
        var query = new GetArticlesQuery<AnonymousArticleDto>(request.SearchTerm, pagination, request.SortBy,
            producerIds, request.SearchStrategy, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleViaStartNumberAnonymousResponse>();
        return Results.Ok(response);
    }
}