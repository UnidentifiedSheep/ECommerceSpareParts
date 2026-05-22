using Abstractions.Interfaces;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Emails;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Auth.AddPermissionToUser;
using Main.Application.Handlers.Users.CreateUser;
using Main.Application.Handlers.Users.GetUsers;
using Main.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Users;

public record AddPermissionToUserRequest(string Permission);

public record CreateUserRequest(
    string UserName,
    string Password,
    UserInfoDto UserInfo,
    IEnumerable<EmailDto> Emails,
    IEnumerable<string> Phones,
    IEnumerable<string> Roles);

public record CreateUserResponse(Guid UserId);

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
    [FromQuery(Name = "searchMethod")] GeneralSearchStrategy SearchMethod) : PaginationQueryModel;

public record GetUsersResponse(IReadOnlyList<UserDto> Users);

public class UsersEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users")
            .WithTags("Users");

        users.MapUserInfoEndPoints();
        users.MapUserStorageEndPoints();

        users.MapPost("/{userId:guid}/permissions/", async (
                ISender sender,
                Guid userId,
                AddPermissionToUserRequest request,
                CancellationToken ct) =>
            {
                await sender.Send(new AddPermissionToUserCommand(userId, request.Permission), ct);
                return Results.NoContent();
            })
            .WithName("AddPermissionToUser")
            .WithSummary("Добавить разрешение пользователю")
            .WithDescription("Добавление пользователю разрешение")
            .WithDisplayName("Добавление пользователю разрешение'")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .RequireAnyPermission(PermissionCodes.USERS_PERMISSIONS_CREATE);

        users.MapPost("/", async (ISender sender, CreateUserRequest request, CancellationToken cancellationToken) =>
            {
                var userId = (await sender.Send(request.Adapt<CreateUserCommand>(), cancellationToken)).UserId;
                return Results.Created($"users/{userId}", new CreateUserResponse(userId));
            })
            .WithName("CreateUser")
            .WithSummary("Создать пользователя")
            .WithDescription("Создание пользователя")
            .WithDisplayName("Создание пользователя")
            .Accepts<CreateUserRequest>(false, "application/json")
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.USERS_CREATE);

        users.MapGet("/", async (
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
                return Results.Ok(result.Adapt<GetUsersResponse>());
            })
            .WithName("GetUsers")
            .WithSummary("Получить пользователей")
            .WithDescription("Получение пользователей")
            .WithDisplayName("Получение пользователей")
            .Produces<GetUsersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.USERS_GET);
    }
}
