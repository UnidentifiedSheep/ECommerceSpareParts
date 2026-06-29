using Abstractions.Interfaces;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Auth;
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

public record CreateUserRequest
{
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required UserInfoDto UserInfo { get; init; }
    public required IEnumerable<EmailDto> Emails { get; init; }
    public required IEnumerable<string> Phones { get; init; }
    public required IEnumerable<string> Roles { get; init; }
}

public record CreateUserResponse(UserDto User);

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
    [FromQuery(Name = "roles")] string[]? Roles,
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
        users.MapUserFinancialEndPoints();
        users.MapUserPermissionEndPoints();
        users.MapUserEmailEndPoints();

        users.MapPost("/", async (ISender sender, CreateUserRequest request, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new CreateUserCommand(
                    request.UserName, 
                    request.Password,
                    request.UserInfo,
                    request.Emails,
                    request.Roles), cancellationToken);
                return Results.Created($"users/{result.User.Id}", new CreateUserResponse(result.User));
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
                    request.Roles,
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
