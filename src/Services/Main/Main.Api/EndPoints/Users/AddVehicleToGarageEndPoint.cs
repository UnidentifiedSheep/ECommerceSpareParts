using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Abstractions.Dtos.Member.Vehicles;
using Main.Application.Handlers.Users.AddVehicleToGarage;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record AddVehicleToGarageRequest(VehicleDto Vehicle);

public class AddVehicleToGarageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId}/vehicles/",
                async (ISender sender, AddVehicleToGarageRequest request, Guid userId, CancellationToken token) =>
                {
                    var command = new AddVehicleToGarageCommand(request.Vehicle, userId);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).WithTags("Users")
                .WithDescription("Добавление транспортного средства пользователю")
                .WithDisplayName("Добавление ТС AMW")
                .RequireAnyPermission(PermissionCodes.USERS_VEHICLES_CREATE_ALL);

        app.MapPost("/users/me/vehicles/", async (ISender sender, AddVehicleToGarageRequest request,
                IUserContext user, CancellationToken token) =>
            {
                var command = new AddVehicleToGarageCommand(request.Vehicle, user.UserId);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("Users")
            .WithDescription("Добавление транспортного средства пользователю")
            .WithDisplayName("Добавление ТС Member")
            .RequireAnyPermission(PermissionCodes.USERS_VEHICLES_CREATE_ME);
    }
}