using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users.AddEmailToUser;
using Main.Application.Handlers.Users.RemoveEmailFromUser;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record AddUserEmailRequest(
    string Email,
    EmailType EmailType);

public record AddUserEmailResponse(UserEmailDto Email);

public static class UserEmailEndPoints
{
    public static RouteGroupBuilder MapUserEmailEndPoints(this RouteGroupBuilder users)
    {
        users.MapPost(
                "/{userId:guid}/emails",
                async (
                    ISender sender,
                    Guid userId,
                    AddUserEmailRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new AddEmailToUserCommand(
                            userId,
                            request.Email,
                            request.EmailType),
                        cancellationToken);

                    return Results.Created(
                        $"/users/{userId}/emails/{Uri.EscapeDataString(request.Email)}",
                        value: new AddUserEmailResponse(result.Email));
                })
            .WithName("AddUserEmail")
            .WithSummary("Добавить почту пользователю")
            .WithDescription("Добавляет пользователю неподтверждённый дополнительный email")
            .WithDisplayName("Добавить почту")
            .Accepts<AddUserEmailRequest>(false, "application/json")
            .Produces<AddUserEmailResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAnyPermission(PermissionCodes.USERS_MAILS_CREATE);

        users.MapDelete(
                "/{userId:guid}/emails/{email}",
                async (
                    ISender sender,
                    Guid userId,
                    string email,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new RemoveEmailFromUserCommand(userId, email), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("RemoveUserEmail")
            .WithSummary("Удаление почты у пользователя")
            .WithDescription("Удаление почты у пользователя")
            .WithDisplayName("Удалить почту")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.USERS_MAILS_CREATE);

        return users;
    }
}
