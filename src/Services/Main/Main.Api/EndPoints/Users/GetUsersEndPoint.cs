using Abstractions.Interfaces;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users.GetUsers;
using Main.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Users;

public record GetUsersRequest(
    [FromQuery(Name = "searchTerm")]
    string? SearchTerm,
    [FromQuery(Name = "id")]
    Guid? Id,
    [FromQuery(Name = "name")]
    string? Name,
    [FromQuery(Name = "surname")]
    string? Surname,
    [FromQuery(Name = "email")]
    string? Email,
    [FromQuery(Name = "phone")]
    string? Phone,
    [FromQuery(Name = "userName")]
    string? UserName,
    [FromQuery(Name = "isSupplier")]
    bool? IsSupplier,
    [FromQuery(Name = "description")]
    string? Description,
    [FromQuery(Name = "similarityLevel")]
    double? SimilarityLevel,
    [FromQuery(Name = "searchMethod")]
    GeneralSearchStrategy SearchMethod) : PaginationQueryModel;

public record GetUsersResponse(IReadOnlyList<UserDto> Users);

public class GetUsersEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/",
                async (
                    ISender sender,
                    [AsParameters] GetUsersRequest request,
                    IUserContext userContext,
                    CancellationToken token) =>
                {
                    var query = new GetUsersQuery(
                        request,
                        request.SearchTerm,
                        request.SimilarityLevel,
                        userContext.UserId,
                        request.Name,
                        request.Surname,
                        request.Email,
                        request.Phone,
                        request.UserName,
                        request.Id,
                        request.Description,
                        [],
                        request.SearchMethod);
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetUsersResponse>();
                    return Results.Ok(response);
                }).WithTags("Users")
            .WithDescription("Получение пользователей")
            .WithDisplayName("Получение пользователей")
            .RequireAnyPermission(PermissionCodes.USERS_GET);
    }
}