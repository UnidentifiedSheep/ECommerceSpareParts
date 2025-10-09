using System.Security.Claims;
using Main.Application.Handlers.Users.GetUsers;
using Carter;
using Core.Dtos.Amw.Users;
using Core.Enums;
using Core.Models;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Users;

public record GetUsersRequest(
    [FromQuery(Name = "searchTerm")] string? SearchTerm,
    [FromQuery(Name = "id")] Guid? Id,
    [FromQuery(Name = "name")] string? Name,
    [FromQuery(Name = "surname")] string? Surname,
    [FromQuery(Name = "email")] string? Email,
    [FromQuery(Name = "phone")] string? Phone,
    [FromQuery(Name = "userName")] string? UserName,
    [FromQuery(Name = "isSupplier")] bool? IsSupplier,
    [FromQuery(Name = "description")] string? Description,
    [FromQuery(Name = "similarityLevel")] double? SimilarityLevel,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "viewCount")] int ViewCount,
    [FromQuery(Name = "searchMethod")] string SearchMethod);

public record GetUsersResponse(IEnumerable<UserDto> Users);

public class GetUsersEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/",
                async (ISender sender, [AsParameters] GetUsersRequest request, ClaimsPrincipal claims,
                    CancellationToken token) =>
                {
                    var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId == null) return Results.Unauthorized();
                    var pagination = new PaginationModel(request.Page, request.ViewCount);
                    var query = new GetUsersQuery(request.SearchTerm, pagination, request.SimilarityLevel,
                        userId, request.Name, request.Surname, request.Email, request.Phone, request.UserName,
                        request.Id,
                        request.Description, request.IsSupplier, Enum.Parse<GeneralSearchStrategy>(request.SearchMethod));
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetUsersResponse>();
                    return Results.Ok(response);
                }).RequireAuthorization("AMW")
            .WithTags("Users")
            .WithDescription("Получение пользователей")
            .WithDisplayName("Получение пользователей");
    }
}