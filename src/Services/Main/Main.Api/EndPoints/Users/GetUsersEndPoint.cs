using System.Security.Claims;
using Api.Common.Extensions;
using Carter;
using Core.Models;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Application.Handlers.Users.GetUsers;
using Main.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Security.Extensions;

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
    [FromQuery(Name = "limit")] int Limit,
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
                    if (!claims.GetUserId(out var userId)) return Results.Unauthorized();
                    
                    var pagination = new PaginationModel(request.Page, request.Limit);
                    var query = new GetUsersQuery(request.SearchTerm, pagination, request.SimilarityLevel,
                        userId, request.Name, request.Surname, request.Email, request.Phone, request.UserName,
                        request.Id,
                        request.Description, request.IsSupplier,
                        Enum.Parse<GeneralSearchStrategy>(request.SearchMethod));
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetUsersResponse>();
                    return Results.Ok(response);
                }).WithTags("Users")
                .WithDescription("Получение пользователей")
                .WithDisplayName("Получение пользователей")
                .RequireAnyPermission(PermissionCodes.USERS_GET);
    }
}