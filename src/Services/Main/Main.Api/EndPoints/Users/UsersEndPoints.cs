using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Emails;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users.CreateUser;
using Main.Application.Handlers.Users.GetUserFullInfo;
using Main.Application.Handlers.Users.GetUsers;
using Main.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Users;

public record CreateUserRequest
{
    [JsonPropertyName("userName")]
    public required string UserName { get; init; }

    [JsonPropertyName("password")]
    public required string Password { get; init; }

    [JsonPropertyName("userInfo")]
    public required UserInfoDto UserInfo { get; init; }

    [JsonPropertyName("emails")]
    public required IEnumerable<EmailDto> Emails { get; init; }

    [JsonPropertyName("phones")]
    public required IEnumerable<UserPhoneDto> Phones { get; init; }

    [JsonPropertyName("roles")]
    public required IEnumerable<string> Roles { get; init; }
}

public record CreateUserResponse(UserDto User);

public record GetUsersRequest : PaginationQueryModel
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "id")]
    public Guid? Id { get; init; }

    [FromQuery(Name = "name")]
    public string? Name { get; init; }

    [FromQuery(Name = "surname")]
    public string? Surname { get; init; }

    [FromQuery(Name = "email")]
    public string? Email { get; init; }

    [FromQuery(Name = "phone")]
    public string? Phone { get; init; }

    [FromQuery(Name = "userName")]
    public string? UserName { get; init; }

    [FromQuery(Name = "isSupplier")]
    public bool? IsSupplier { get; init; }

    [FromQuery(Name = "description")]
    public string? Description { get; init; }

    [FromQuery(Name = "similarityLevel")]
    public double? SimilarityLevel { get; init; }

    [FromQuery(Name = "roles")]
    public string[]? Roles { get; init; }

    [FromQuery(Name = "searchMethod")]
    public GeneralSearchStrategy SearchMethod { get; init; }
}

public record GetUsersResponse(IReadOnlyList<UserDto> Users);

public class UsersEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users")
            .WithTags("Users");

        users.MapUserInfoEndPoints();
        users.MapUserStorageEndPoints();
        users.MapUserDiscountEndPoints();
        users.MapUserPermissionEndPoints();
        users.MapUserEmailEndPoints();
        users.MapUserRoleEndPoints();
        users.MapUserOrganizationEndPoints();

        users.MapPost(
                "/",
                async (
                    ISender sender,
                    CreateUserRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new CreateUserCommand(
                            request.UserName,
                            request.Password,
                            request.UserInfo,
                            request.Emails,
                            request.Phones,
                            request.Roles),
                        cancellationToken);
                    var user = await sender.Send(
                        new GetUserFullInfoQuery(result.UserId),
                        cancellationToken);

                    return Results.Created(
                        $"/users/{result.UserId}",
                        new CreateUserResponse(user.User));
                })
            .WithName("CreateUser")
            .WithSummary("Создать пользователя")
            .WithDescription("Создание пользователя")
            .WithDisplayName("Создание пользователя")
            .Accepts<CreateUserRequest>(false, "application/json")
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.USERS_CREATE);

        users.MapGet(
                "/",
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
                        request.Roles,
                        request.SearchMethod);
                    var result = await sender.Send(query, token);
                    return Results.Ok(new GetUsersResponse(result.Users));
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
