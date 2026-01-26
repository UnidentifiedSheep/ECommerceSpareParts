using Api.Common.Extensions;
using Carter;
using Core.Models;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Application.Handlers.Users.GetUserStorages;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record GetUserStoragesResponse(List<StorageDto> Storages);

public class GetUserStoragesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{userId:guid}/storages", 
                async (ISender sender, Guid userId, int page, int limit, CancellationToken token) =>
        {
            var query = new GetUserStoragesQuery(userId, new PaginationModel(page, limit));
            var result = await sender.Send(query, token);
            return Results.Ok(new GetUserStoragesResponse(result.Storages));
        }).WithTags("Users")
        .WithDescription("Получение складов привязанных к пользователю.")
        .WithDisplayName("Получение складов пользователя.")
        .RequireAnyPermission(PermissionCodes.USERS_STORAGES_GET);
    }
}