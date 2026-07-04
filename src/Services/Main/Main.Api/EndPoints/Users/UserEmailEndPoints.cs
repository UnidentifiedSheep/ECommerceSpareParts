using Api.Common.Extensions;
using Enums;
using Main.Application.Handlers.Users.RemoveEmailFromUser;
using MediatR;

namespace Main.Api.EndPoints.Users;

public static class UserEmailEndPoints
{
    public static RouteGroupBuilder MapUserEmailEndPoints(this RouteGroupBuilder users)
    {
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